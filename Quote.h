#ifndef QUOTE_H
#define QUOTE_H

#include "TapQuoteAPI.h"
#include "SimpleEvent.h"


class Quote : public ITapQuoteAPINotify
{
public:
	Quote(void);
	~Quote(void);

	void SetAPI(ITapQuoteAPI* pAPI);
	void Run();
	static	void  ConnectRedis();

public:
	//��ITapQuoteAPINotify��ʵ��
	virtual void TAP_CDECL OnRspLogin(TAPIINT32 errorCode, const TapAPIQuotLoginRspInfo *info);
	virtual void TAP_CDECL OnAPIReady();
	virtual void TAP_CDECL OnDisconnect(TAPIINT32 reasonCode);
	virtual void TAP_CDECL OnRspQryCommodity(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteCommodityInfo *info);
	virtual void TAP_CDECL OnRspQryContract(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteContractInfo *info);
	virtual void TAP_CDECL OnRspSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIQuoteWhole *info);
	virtual void TAP_CDECL OnRspUnSubscribeQuote(TAPIUINT32 sessionID, TAPIINT32 errorCode, TAPIYNFLAG isLast, const TapAPIContract *info);
	virtual void TAP_CDECL OnRtnQuote(const TapAPIQuoteWhole *info);

private:

	ITapQuoteAPI* m_pAPI;
	TAPIUINT32	m_uiSessionID;
	SimpleEvent m_Event;
	bool		m_bIsAPIReady;

};

#endif // QUOTE_H
