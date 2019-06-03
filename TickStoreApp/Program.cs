using Core;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TickStoreApp
{
    class Program
    {
        static async Task RunClientAsync()
        {
            int a = 32;
            int b = 23;
            int c = a | b;
            c = a & b;
            c = a ^ b;
            var bin = Convert.ToString(79, 2);
            var dec = Convert.ToString(79, 8);
            var hex = Convert.ToString(79, 16);

            byte[] bytes = BitConverter.GetBytes(17);
            byte[] data = new byte[] { byte.MinValue, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 254, byte.MaxValue };

            string s = BitConverter.ToString(data);

            var items = new List<MySqlConnection>();
            int x = 0;
            while (x < 500)
            {
                x++;
                MySqlConnection connection = new MySqlConnection("Server=47.98.226.195; database=world; UID=nginx; password=nginx; SSLMode=none");
                connection.Open();
                items.Add(connection);
            }
            Console.WriteLine($"Conn Count:{items.Count}");

            Console.ReadLine();

            // int x = MySqlHelper.ExecuteNonQuery(connection, "delete from x");


            IPAddress Host = IPAddress.Parse(ServerSettings.Host);
            IPEndPoint endpoint = new IPEndPoint(Host, ServerSettings.Port);
            Console.Title = endpoint.ToString();
            MultithreadEventLoopGroup group = new MultithreadEventLoopGroup();

            X509Certificate2 cert = null;
            string targetHost = null;
            if (ServerSettings.IsSsl)
            {
                cert = new X509Certificate2(Path.Combine(ServerSettings.X509Cert, "dotnetty.com.pfx"), "password");
                targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            }
            try
            {
                Bootstrap bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        if (cert != null)
                        {
                            pipeline.AddLast(new TlsHandler(stream => new SslStream(
                                stream, true, (sender, certificate, chain, errors) => true),
                                new ClientTlsSettings(targetHost)));
                        }

                        pipeline.AddLast(new DelimiterBasedFrameDecoder(8192, Delimiters.LineDelimiter()));
                        pipeline.AddLast(
                            new StringEncoder(Encoding.UTF8),
                            new StringDecoder(Encoding.UTF8),
                            new TickSubscribeHandler()
                            );
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(endpoint);

                for (; ; )
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    try
                    {
                        await bootstrapChannel.WriteAndFlushAsync(line + "\r\n");
                    }
                    catch
                    {
                    }
                    if (string.Equals(line, "bye", StringComparison.OrdinalIgnoreCase))
                    {
                        await bootstrapChannel.CloseAsync();
                        break;
                    }
                }
                //await bootstrapChannel.CloseAsync();
            }
            finally
            {
                //group.ShutdownGracefullyAsync().Wait(1000);
            }
        }
        static void Main() =>
            //Console.WriteLine("正在启动……");
            RunClientAsync().Wait();
    }
}
