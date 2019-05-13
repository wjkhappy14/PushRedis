
using System;
using System.IO;

namespace SignalR.TickService.Client.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            TextWriter writer = Console.Out;
            CommonClient client = new CommonClient(writer);
            client.Run("http://localhost/");

            Console.ReadKey();
        }
    }
}