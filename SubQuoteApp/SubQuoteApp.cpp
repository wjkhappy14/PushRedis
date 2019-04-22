// SubQuoteApp.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include "pch.h"	   
#include <cpp_redis/cpp_redis>
#include <tacopie/tacopie>

#include <condition_variable>
#include <iostream>
#include <mutex>
#include <signal.h>

#ifdef _WIN32
#include <Winsock2.h>
using namespace std::chrono;
#endif /* _WIN32 */

std::condition_variable should_exit;

void
sigint_handler(int) {
	should_exit.notify_all();
}

int
main(void) {

	printf("\x1B[35mTexting\033[0m\n");

	const WORD colors[] =
	{
	0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F,
	0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6
	};

	HANDLE hstdin = GetStdHandle(STD_INPUT_HANDLE);
	HANDLE hstdout = GetStdHandle(STD_OUTPUT_HANDLE);
	WORD   index = 0;

	// Remember how things were when we started
	CONSOLE_SCREEN_BUFFER_INFO csbi;
	GetConsoleScreenBufferInfo(hstdout, &csbi);

	// Tell the user how to stop
	SetConsoleTextAttribute(hstdout, 0xEC);
	std::cout << "Press any key to quit.\n";


#ifdef _WIN32
	//! Windows netword DLL init
	WORD version = MAKEWORD(2, 2);
	WSADATA data;

	if (WSAStartup(version, &data) != 0) {
		std::cerr << "WSAStartup() failure" << std::endl;
		return -1;
	}
#endif /* _WIN32 */

	//! Enable logging
	cpp_redis::active_logger = std::unique_ptr<cpp_redis::logger>(new cpp_redis::logger);

	cpp_redis::subscriber sub;
	sub.connect("47.98.226.195", 55019, [](const std::string & host, std::size_t port, cpp_redis::subscriber::connect_state status) {
		if (status == cpp_redis::subscriber::connect_state::dropped) {
			std::cout << "client disconnected from " << host << ":" << port << std::endl;
			should_exit.notify_all();
		}
		else if (status == cpp_redis::subscriber::connect_state::ok)
		{
			std::cout << "client connected from " << host << ":" << port << std::endl;
		}
	});

	//! authentication if server-server requires it
	/* sub.auth("03hx5DDDivYmbkTgDlFz", [](const cpp_redis::reply& reply) {
	   if (reply.is_error()) { std::cerr << "Authentication failed: " << reply.as_string() << std::endl; }
	   else {
		 std::cout << "successful authentication" << std::endl;
	   }
	 });
*/
	sub.subscribe("now", [](const std::string & chan, const std::string & msg) {
		std::cout << "MESSAGE " << chan << ": " << msg << std::endl;
	});
	sub.commit();

	signal(SIGINT, &sigint_handler);
	std::mutex mtx;
	std::unique_lock<std::mutex> l(mtx);
	should_exit.wait(l);

#ifdef _WIN32
	WSACleanup();
#endif /* _WIN32 */

	return 0;
}