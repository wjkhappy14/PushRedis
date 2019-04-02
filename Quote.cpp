#include "Quote.h"
#include <algorithm>
#include "TapAPIError.h"
#include "QuoteConfig.h"
#include<time.h>
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
		const char *password = "1234QWERasdf";
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
			cout << "连接到 Redis" << hostname << ":" << port << endl;
		}
		reply = (redisReply *)redisCommand(redisCTX, "AUTH %s", password);
	}
	void Quote::Run()
	{
		ConnectRedis();
		if (NULL == m_pAPI) {
			cout << "Error: m_pAPI is NULL." << endl;
			return;
		}
		TAPIINT32 iErr = TAPIERROR_SUCCEED;

		//设定服务器IP、端口
		iErr = m_pAPI->SetHostAddress(DEFAULT_IP, DEFAULT_PORT);
		if (TAPIERROR_SUCCEED != iErr) {
			cout << "SetHostAddress Error:" << iErr << endl;
			return;
		}
		//登录服务器
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
		//等待APIReady
		m_Event.WaitEvent();
		if (!m_bIsAPIReady) {
			return;
		}
		m_uiSessionID = 0;
		//m_pAPI->QryCommodity(&m_uiSessionID);
		SubscribeItems();
	}

	void Quote::SubscribeItems()
	{
		TAPIINT32 iErr = TAPIERROR_SUCCEED;
		m_uiSessionID = 0;
		TapAPIContract items[1];
		TapAPIContract stContract;
		memset(&stContract, 0, sizeof(stContract));

		strcpy(stContract.Commodity.ExchangeNo, std::string("HKEX").c_str());
		stContract.Commodity.CommodityType = TAPI_COMMODITY_TYPE_FUTURES;
		strcpy(stContract.Commodity.CommodityNo, std::string("HSI").c_str());
		strcpy(stContract.ContractNo1, std::string("1904").c_str());

		stContract.CallOrPutFlag1 = TAPI_CALLPUT_FLAG_NONE;
		stContract.CallOrPutFlag2 = TAPI_CALLPUT_FLAG_NONE;
		m_uiSessionID = 0;

		items[0] = stContract;
		iErr = m_pAPI->SubscribeQuote(&m_uiSessionID, &items[0]);
		std::string msg("订阅行情");
		switch (iErr)
		{
		case TAPIERROR_SUBSCRIBEQUOTE_MAX:
			msg += "超过行情最大总订阅数";
			break;
		case TAPIERROR_SUBSCRIBEQUOTE_EXCHANGE_MAX:
			msg += "超过该交易所行情最大订阅数";
			break;
		case TAPIERROR_SUBSCRIBEQUOTE_NO_RIGHT:
			msg += "没有该行情的订阅权限";
			break;
		case TAPIERROR_SUBSCRIBEQUOTE_NO_EXCHANGE_RIGHT:
			msg += "没有该交易所下行情的订阅权限";
			break;
		case TAPIERROR_SUBSCRIBEQUOTE_COMMODITY_NOT_EXIST:
			msg += "品种不存在";
			break;
		case TAPIERROR_SUBSCRIBEQUOTE_CONTRACT_MAY_NOT_EXIST:
			msg += "合约可能不存在";
			break;
		case TAPIERROR_QUOTEFRONT_UNKNOWN_PROTOCOL:
			msg += "不支持的行情协议";
			break;
		default:
			break;
		}
		cout << msg << iErr << endl;
	}
	void TAP_CDECL Quote::OnRspLogin(TAPIINT32 errorCode, const TapAPIQuotLoginRspInfo *info)
	{
		if (TAPIERROR_SUCCEED == errorCode) {
			cout << "登录成功，等待API初始化...UserNo:" << info->StartTime << endl;
			redisReply* reply = (redisReply*)redisCommand(redisCTX, "SET Login_UserNo %s", info->UserNo, 20);
			freeReplyObject(reply);
			m_bIsAPIReady = true;
		}
		else {
			cout << "登录失败，错误码:" << errorCode << endl;
			m_Event.SignalEvent();
		}
	}
	void TAP_CDECL Quote::OnAPIReady()
	{
		cout << "API初始化完成" << endl;
		m_Event.SignalEvent();
	}

	void TAP_CDECL Quote::OnDisconnect(TAPIINT32 reasonCode)
	{
		cout << "API断开,断开原因:" << reasonCode << endl;
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
	//订阅行情
	void TAP_CDECL Quote::OnRspQryContract(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteContractInfo *info)
	{
		TAPIINT32 iErr = TAPIERROR_SUCCEED;
		cout << __FUNCTION__ << " is called." << endl;
		if (NULL != info)
		{
			cout << "合约:" << info->Contract.Commodity.CommodityNo << info->Contract.ContractNo1 << endl;
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
			std::string msg("订阅行情");
			switch (iErr)
			{
			case TAPIERROR_SUBSCRIBEQUOTE_MAX:
				msg += "超过行情最大总订阅数";
				break;
			case TAPIERROR_SUBSCRIBEQUOTE_EXCHANGE_MAX:
				msg += "超过该交易所行情最大订阅数";
				break;
			case TAPIERROR_SUBSCRIBEQUOTE_NO_RIGHT:
				msg += "没有该行情的订阅权限";
				break;
			case TAPIERROR_SUBSCRIBEQUOTE_NO_EXCHANGE_RIGHT:
				msg += "没有该交易所下行情的订阅权限";
				break;
			case TAPIERROR_SUBSCRIBEQUOTE_COMMODITY_NOT_EXIST:
				msg += "品种不存在";
				break;
			case TAPIERROR_SUBSCRIBEQUOTE_CONTRACT_MAY_NOT_EXIST:
				msg += "合约可能不存在";
				break;
			case TAPIERROR_QUOTEFRONT_UNKNOWN_PROTOCOL:
				msg += "不支持的行情协议";
				break;
			default:
				break;
			}
			cout << msg << iErr << endl;
			/*while (true) {
				m_Event.WaitEvent();
			}*/
		}
	}
	void TAP_CDECL Quote::OnRspSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteWhole *info)
	{
		if (TAPIERROR_SUCCEED == errorCode)
		{
			cout << "行情订阅成功 ";
			if (NULL != info)
			{
				std::string dateTimeStamp(info->DateTimeStamp);
				std::string exchangeNo(info->Contract.Commodity.ExchangeNo);
				std::string commodityNo(info->Contract.Commodity.CommodityNo);
				std::string contractNo1(info->Contract.ContractNo1);
				std::string exchange_contract = exchangeNo + commodityNo + contractNo1;
				std::string keyvalue_cmd = "SET " + exchangeNo + ":Sub:" + commodityNo + "  " + commodityNo + contractNo1;

				redisReply* reply = (redisReply*)redisCommand(redisCTX, keyvalue_cmd.c_str());
				cout << exchange_contract << endl;
			}
		}
		else {
			cout << "行情订阅失败，错误码：" << errorCode << endl;
		}
	}

	void TAP_CDECL Quote::OnRspUnSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIContract *info)
	{
		cout << __FUNCTION__ << " is called." << endl;
	}
	void TAP_CDECL Quote::OnRtnQuote(const TapAPIQuoteWhole *info)
	{
		time_t rawtime;
		struct tm * timeinfo;
		char buffer[80];
		time(&rawtime);
		timeinfo = localtime(&rawtime);

		strftime(buffer, sizeof(buffer), "%d-%m-%Y %H:%M:%S", timeinfo);
		std::string now(buffer);
		cout << "local time:" << now << endl;
		if (NULL != info)
		{
			std::string dateTimeStamp(info->DateTimeStamp);
			std::string exchangeNo(info->Contract.Commodity.ExchangeNo);
			std::string commodityNo(info->Contract.Commodity.CommodityNo);
			std::string contractNo1(info->Contract.ContractNo1);
			std::replace(dateTimeStamp.begin(), dateTimeStamp.end(), ' ', ':');
			std::replace(dateTimeStamp.begin(), dateTimeStamp.end(), '-', ':');
			std::replace(dateTimeStamp.begin(), dateTimeStamp.end(), '.', ':');

			std::string   pub_tickKey = "  " + exchangeNo + ":" + commodityNo + ":" + contractNo1 + "  ";
			std::string   set_tickKey = "  " + exchangeNo + ":" + commodityNo + ":" + contractNo1 + ":" + dateTimeStamp + "  ";
			std::string   tickValue = dateTimeStamp + "," + std::to_string(info->QLastPrice) + "," + std::to_string(info->QTotalQty) + "," + std::to_string(info->QLastQty);

			std::string  publish_cmd = "PUBLISH " + pub_tickKey + tickValue;
			redisReply* pubReply = (redisReply*)redisCommand(redisCTX, publish_cmd.c_str());

			std::string set_tick_cmd = "SET  " + set_tickKey + tickValue;
			redisReply*	setReply = (redisReply*)redisCommand(redisCTX, set_tick_cmd.c_str());

			char hkey[] = "123456";
			char hset[] = "hset";
			char key[] = "testkey";
			char hvalue[] = "3210";
			int argc = 4;
			char *argv[] = { hset,key,hkey,hvalue };
			size_t argvlen[] = { 4,6,4,3 };
			//redisCommandArgv(redisCTX, argc, argv, argvlen);

			//每一次执行完Redis命令后需要清空redisReply 以免对下一次的Redis操作造成影响
			if (pubReply != NULL)
			{
				freeReplyObject(pubReply);
			}
			if (setReply != NULL)
			{
				freeReplyObject(setReply);
			}
			cout << publish_cmd << endl;
		}
	}
}


