#include "Quote.h"
#include "TapAPIError.h"
#include "QuoteConfig.h"
//#include <Windows.h>

#include <iostream>
#include <string.h>
#include <hiredis.h>

using namespace std;

namespace QuotePushRedis
{
	static redisContext *redisCTX;
	Quote::Quote(void) :
		m_pAPI(NULL),
		m_bIsAPIReady(false)
	{
	}

	Quote::~Quote(void)
	{
	}

	void Quote::SetAPI(ITapQuoteAPI *pAPI)
	{
		m_pAPI = pAPI;
	}

	void Quote::ConnectRedis()
	{
		redisReply *reply;
		const char *password = "123456";
		const char *hostname = "114.67.236.124"; //114.67.236.124
		int port = 6379;

		struct timeval timeout = { 1, 500000 }; // 1.5 seconds
		redisCTX = redisConnectWithTimeout(hostname, port, timeout);
		if (redisCTX == NULL || redisCTX->err) {
			if (redisCTX) {
				printf("Connection error: %s\n", redisCTX->errstr);
				//redisFree(c);
			}
			else {
				cout << "Connection error: can't allocate redis context\n";
			}
		}
		else
		{
			cout << "���ӵ� Redis" << hostname << ":" << port << endl;
		}
	}
	void Quote::Run()
	{
		ConnectRedis();
		if (NULL == m_pAPI) {
			cout << "Error: m_pAPI is NULL." << endl;
			return;
		}
		TAPIINT32 iErr = TAPIERROR_SUCCEED;

		//�趨������IP���˿�
		iErr = m_pAPI->SetHostAddress(DEFAULT_IP, DEFAULT_PORT);
		if (TAPIERROR_SUCCEED != iErr) {
			cout << "SetHostAddress Error:" << iErr << endl;
			return;
		}
		//��¼������
		TapAPIQuoteLoginAuth stLoginAuth;
		memset(&stLoginAuth, 0, sizeof(stLoginAuth));
		strcpy(stLoginAuth.UserNo, DEFAULT_USERNAME);
		strcpy(stLoginAuth.Password, DEFAULT_PASSWORD);
		stLoginAuth.ISModifyPassword = APIYNFLAG_NO;
		stLoginAuth.ISDDA = APIYNFLAG_NO;
		iErr = m_pAPI->Login(&stLoginAuth);
		if (TAPIERROR_SUCCEED != iErr) {
			cout << "Login Error:" << iErr << endl;
			return;
		}
		//�ȴ�APIReady
		m_Event.WaitEvent();
		if (!m_bIsAPIReady) {
			return;
		}
		m_uiSessionID = 0;
		m_pAPI->QryCommodity(&m_uiSessionID);
	}
	void TAP_CDECL Quote::OnRspLogin(TAPIINT32 errorCode, const TapAPIQuotLoginRspInfo *info)
	{
		if (TAPIERROR_SUCCEED == errorCode) {
			cout << "��¼�ɹ����ȴ�API��ʼ��...UserNo:" << info->StartTime << endl;
			redisReply* reply = (redisReply*)redisCommand(redisCTX, "SET Login_UserNo %s", info->UserNo, 20);
			freeReplyObject(reply);
			m_bIsAPIReady = true;
		}
		else {
			cout << "��¼ʧ�ܣ�������:" << errorCode << endl;
			m_Event.SignalEvent();
		}
	}
	void TAP_CDECL Quote::OnAPIReady()
	{
		cout << "API��ʼ�����" << endl;
		m_Event.SignalEvent();
	}

	void TAP_CDECL Quote::OnDisconnect(TAPIINT32 reasonCode)
	{
		cout << "API�Ͽ�,�Ͽ�ԭ��:" << reasonCode << endl;
	}

	void TAP_CDECL Quote::OnRspQryCommodity(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteCommodityInfo *info)
	{
		cout << __FUNCTION__ << " is called." << endl;
		std::string exchangeNo(info->Commodity.ExchangeNo);
		std::string commodityNo(info->Commodity.CommodityNo);
		std::string commodityEngName(info->CommodityEngName);

		std::string exchange_commodity_key = exchangeNo + commodityNo;
		std::string exchange_commodity_value = exchangeNo + commodityNo + commodityEngName;
		std::string keyvalue_cmd = "SET " + exchange_commodity_key + "   " + exchange_commodity_value;

		redisReply* reply = (redisReply*)redisCommand(redisCTX, keyvalue_cmd.c_str());

		TapAPICommodity com;
		memset(&com, 0, sizeof(com));
		strcpy(com.ExchangeNo, info->Commodity.ExchangeNo);
		strcpy(com.CommodityNo, info->Commodity.CommodityNo);
		com.CommodityType = info->Commodity.CommodityType;

		m_pAPI->QryContract(&m_uiSessionID, &com);

		cout << exchange_commodity_key << endl;
	}
	//��������
	void TAP_CDECL Quote::OnRspQryContract(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteContractInfo *info)
	{
		TAPIINT32 iErr = TAPIERROR_SUCCEED;
		cout << __FUNCTION__ << " is called." << endl;
		if (NULL != info)
		{
			cout << "��Լ:" << info->Contract.Commodity.CommodityNo << info->Contract.ContractNo1 << endl;
			std::string exchangeNo(info->Contract.Commodity.ExchangeNo);
			std::string commodityNo(info->Contract.Commodity.CommodityNo);
			std::string contractNo1(info->Contract.ContractNo1);

			std::string exchange_commodity_key = exchangeNo + commodityNo + contractNo1;
			std::string exchange_commodity_value = exchangeNo + commodityNo + contractNo1;
			std::string keyvalue_cmd = "SET " + exchange_commodity_key + "   " + exchange_commodity_value;

			redisReply* reply = (redisReply*)redisCommand(redisCTX, keyvalue_cmd.c_str());

			TapAPIContract stContract;
			memset(&stContract, 0, sizeof(stContract));

			strcpy(stContract.Commodity.ExchangeNo, info->Contract.Commodity.ExchangeNo);
			stContract.Commodity.CommodityType = info->Contract.Commodity.CommodityType;
			strcpy(stContract.Commodity.CommodityNo, info->Contract.Commodity.CommodityNo);

			strcpy(stContract.ContractNo1, info->Contract.ContractNo1);
			stContract.CallOrPutFlag1 = TAPI_CALLPUT_FLAG_NONE;
			stContract.CallOrPutFlag2 = TAPI_CALLPUT_FLAG_NONE;
			m_uiSessionID = 0;
			iErr = m_pAPI->SubscribeQuote(&m_uiSessionID, &stContract);
			if (TAPIERROR_SUCCEED != iErr) {
				cout << "SubscribeQuote Error:" << iErr << endl;
				return;
			}
			while (true) {
				m_Event.WaitEvent();
			}
		}
	}
	void TAP_CDECL Quote::OnRspSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteWhole *info)
	{
		if (TAPIERROR_SUCCEED == errorCode)
		{
			cout << "���鶩�ĳɹ� ";
			if (NULL != info)
			{
				std::string dateTimeStamp(info->DateTimeStamp);
				std::string exchangeNo(info->Contract.Commodity.ExchangeNo);
				std::string commodityNo(info->Contract.Commodity.CommodityNo);
				std::string contractNo1(info->Contract.ContractNo1);
				std::string exchange_contract = exchangeNo + commodityNo + contractNo1;
				redisReply* reply = (redisReply*)redisCommand(redisCTX, "SET SubscribeQuote %s", exchange_contract, 10);
				cout << exchange_contract << endl;
			}
		}
		else {
			cout << "���鶩��ʧ�ܣ������룺" << errorCode << endl;
		}
	}

	void TAP_CDECL Quote::OnRspUnSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIContract *info)
	{
		cout << __FUNCTION__ << " is called." << endl;
	}

	void TAP_CDECL Quote::OnRtnQuote(const TapAPIQuoteWhole *info)
	{
		if (NULL != info)
		{
			char buff[100];
			snprintf(buff, sizeof(buff), "%s", "Hello");
			std::string buffAsStdStr = buff;

			std::string dateTimeStamp(info->DateTimeStamp);
			std::string commodityNo(info->Contract.Commodity.CommodityNo);
			std::string contractNo1(info->Contract.ContractNo1);
			std::string contractKey = commodityNo + contractNo1;

			auto lastPrice = QuotePushRedis::Helper::to_string(info->QLastPrice);
			std::string  publish_cmd = "publish " + contractKey + " " + lastPrice;
			redisReply* reply = (redisReply*)redisCommand(redisCTX, publish_cmd.c_str());

			std::string LastPricecmd = "SET LastPrice  " + lastPrice;
			reply = (redisReply*)redisCommand(redisCTX, LastPricecmd.c_str());

			redisReply *pubReply = (redisReply*)redisCommand(redisCTX, "SET DateTimeStamp %s ", info->DateTimeStamp, 24);

			char hkey[] = "123456";
			char hset[] = "hset";
			char key[] = "testkey";
			char hvalue[] = "3210";
			int argc = 4;
			char *argv[] = { hset,key,hkey,hvalue };
			size_t argvlen[] = { 4,6,4,3 };

			//redisCommandArgv(redisCTX, argc, argv, argvlen);

			//ÿһ��ִ����Redis�������Ҫ���redisReply �������һ�ε�Redis�������Ӱ��
			freeReplyObject(reply);

			cout << "�������:"
				<< info->DateTimeStamp << " "
				<< info->Contract.Commodity.ExchangeNo << " "
				<< info->Contract.Commodity.CommodityType << " "
				<< info->Contract.Commodity.CommodityNo << " "
				<< info->Contract.ContractNo1 << " "
				<< info->QLastPrice
				// ...		
				<< endl;
		}
	}
}


