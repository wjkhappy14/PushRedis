#include "TapQuoteAPI.h"
#include "TapAPIError.h"
#include "Quote.h"
#include "QuoteConfig.h"

#include "pch.h"
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include<WinSock2.h>
#include <hiredis.h>
#include"ConsoleApp.h"

using namespace std;

int main(int argc, char **argv) {

	cout << GetTapQuoteAPIVersion() << endl;

	unsigned int j;
	redisContext *c;
	redisReply *reply;
	const char *password = (argc > 1) ? argv[3] : "123456";
	const char *hostname = (argc > 1) ? argv[1] : "114.67.236.124";
	int port = (argc > 2) ? atoi(argv[2]) : 6379;

	struct timeval timeout = { 1, 500000 }; // 1.5 seconds
	c = redisConnectWithTimeout(hostname, port, timeout);
	int retval = redisAppendCommand(c, "SET Name Angkor");

	TAPIINT32 iResult = TAPIERROR_SUCCEED;
	TapAPIApplicationInfo stAppInfo;
	strcpy(stAppInfo.AuthCode, DEFAULT_AUTHCODE);
	strcpy(stAppInfo.KeyOperationLogPath, "");
	ITapQuoteAPI *pAPI = CreateTapQuoteAPI(&stAppInfo, iResult);
	if (NULL == pAPI) {
		cout << "........" << iResult << endl;
		//	return -1;
	}

	Quote objQuote;
	pAPI->SetAPINotify(&objQuote);

	objQuote.SetAPI(pAPI);
	objQuote.RunTest();

	getchar();
	return 0;
}