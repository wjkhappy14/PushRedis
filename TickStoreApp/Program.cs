using Core;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
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
