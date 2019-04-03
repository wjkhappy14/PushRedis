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

#include"ConsoleApp.h"
using namespace std;
using namespace QuotePushRedis;


int main(int argc, char **argv) {
	auto hello = std::string{ "Hello!" };
	char* numbers[10] = {  };
	std::string s = "How are you ";

	auto str = hello.c_str();
	TAPISTR_10 str10 = "Hello";

	cout << GetTapQuoteAPIVersion() << endl;

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
	objQuote.Run();

	auto c=getchar();
	return 0;
}