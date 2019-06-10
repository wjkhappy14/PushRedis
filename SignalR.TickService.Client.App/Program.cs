
using Microsoft.AspNet.SignalR.Client;
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
            string wsUrl = "ws://65.52.173.5:2048/ws";
            Connection conn = new Connection(wsUrl);
            conn.Start();
            conn.Send("Hello");

            byte[] hello = Convert.FromBase64String("MDEyMzQ1Njc4OUFCQ0RFRg==");
            ArraySegment<byte> myArrSegMid = new ArraySegment<byte>(hello, 0, hello.Length);

            TextWriter writer = Console.Out;

            ClientWebSocket ws = new ClientWebSocket();
            ws.ConnectAsync(new Uri(wsUrl), new CancellationToken());
            ws.SendAsync(myArrSegMid, WebSocketMessageType.Text, true, new CancellationToken());

            Thread.Sleep(TimeSpan.FromSeconds(1));

            //2019-05-14T10:41:20.2345454+08:00
            //2019-05-14 10:42:57.3252446 yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF
            string now = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF");
            //CommonClient client = new CommonClient(writer);

            // client.Run("http://10.0.1.4/");

            Console.ReadKey();
        }
    }
}
