
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

namespace SignalR.TickService.Client.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TextWriter writer = Console.Out;
            string s = "wss://chat.chatra.io/sockjs/206/0da7n_cx/websocket";
            string me = "wss://www.angkorw.cn/raw-connection/connect?transport=webSockets&clientProtocol=2.1&connectionToken=8GQzSZA4g0LNcdTOJFrK3jk91YUK7n37%2FIvotV7WdaqQgtVStcaCd%2BvMNrvPCd%2F5mg%2B1qMAip9s%2FTklAzXm5xgDPRW7zRjZDYfwuJAX1ng7wJurDQeGSOpEWTCrB%2BmRM&tid=6";

            string whatsappWS = "wss://web.whatsapp.com/ws";
            string local = "ws://10.0.1.4/raw-connection/connect?transport=webSockets&clientProtocol=2.1&connectionToken=I9bEkFBjoAj1%2BhVpBKm3OVoDcBq2H7xm9exkQMBg88ydn6FwBZ%2F1QR203aP%2FAdM9HW6SLPpAkCbwXSHeeGaSSlJ%2BQ20X4dzhph5iF4JwPwAqyHerndau1xrXZosq2D%2Fz&tid=5";
          

            while (true)
            {
                ClientWebSocket ws = new ClientWebSocket();
                ws.ConnectAsync(new Uri(local), new CancellationToken());
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            //2019-05-14T10:41:20.2345454+08:00
            //2019-05-14 10:42:57.3252446 yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF
            string now = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF");

            CommonClient client = new CommonClient(writer);

            client.Run("http://10.0.1.4/");

            Console.ReadKey();
        }
    }
}
