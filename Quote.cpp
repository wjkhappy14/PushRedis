#include "Quote.h"
#include "TapAPIError.h"
#include "QuoteConfig.h"
//#include <Windows.h>
#include <iostream>
#include <string.h>
#include <hiredis.h>
using namespace std;
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
	unsigned int j;

	redisReply *reply;
	const char *password = "123456";
	const char *hostname = "127.0.0.1"; //114.67.236.124
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
	int retval = redisAppendCommand(redisCTX, "SET X-Name XXXX-Angkor");
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
	TapAPICommodity com;
	memset(&com, 0, sizeof(com));
	strcpy(com.ExchangeNo, DEFAULT_EXCHANGE_NO);
	strcpy(com.CommodityNo, DEFAULT_COMMODITY_NO);
	com.CommodityType = DEFAULT_COMMODITY_TYPE;
	m_pAPI->QryContract(&m_uiSessionID, &com);

	//订阅行情
	TapAPIContract stContract;
	memset(&stContract, 0, sizeof(stContract));
	strcpy(stContract.Commodity.ExchangeNo, DEFAULT_EXCHANGE_NO);
	stContract.Commodity.CommodityType = DEFAULT_COMMODITY_TYPE;
	strcpy(stContract.Commodity.CommodityNo, DEFAULT_COMMODITY_NO);
	strcpy(stContract.ContractNo1, DEFAULT_CONTRACT_NO);
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

void TAP_CDECL Quote::OnRspLogin(TAPIINT32 errorCode, const TapAPIQuotLoginRspInfo *info)
{
	if (TAPIERROR_SUCCEED == errorCode) {
		cout << "登录成功，等待API初始化...LastLoginIP:" << info->LastLoginIP << endl;
		m_bIsAPIReady = true;
		redisReply *reply;
		redisCommand(redisCTX, "set  LastLoginIP %s", info->LastLoginIP);
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
}

void TAP_CDECL Quote::OnRspQryContract(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteContractInfo *info)
{
	cout << __FUNCTION__ << " is called." << endl;

	cout << "合约:" << info->Contract.Commodity.CommodityNo << info->Contract.ContractNo1 << endl;
}


void TAP_CDECL Quote::OnRspSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteWhole *info)
{
	if (TAPIERROR_SUCCEED == errorCode)
	{
		cout << "行情订阅成功 ";
		if (NULL != info)
		{
			cout << info->DateTimeStamp << " "
				<< info->Contract.Commodity.ExchangeNo << " "
				<< info->Contract.Commodity.CommodityType << " "
				<< info->Contract.Commodity.CommodityNo << " "
				<< info->Contract.ContractNo1 << " "
				<< info->QLastPrice
				// ...		
				<< endl;
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
	if (NULL != info)
	{
		redisReply* reply = (redisReply*)redisCommand(redisCTX, "set  QLastPrice  123");
		redisReply *pubReply = (redisReply*)redisCommand(redisCTX, "publish time 12312224");
		cout << "行情更新:"
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
