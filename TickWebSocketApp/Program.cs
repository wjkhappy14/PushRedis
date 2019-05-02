
namespace TickWebSocketApp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Core;
    using DotNetty.Codecs.Http;
    using DotNetty.Common;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;

    class Program
    {
    
        static async Task RunServerAsync()
        {

            int processId = Platform.GetCurrentProcessId();
            byte[] deviceId = Platform.GetDefaultDeviceId();

            Console.WriteLine($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            Console.WriteLine($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            Console.WriteLine("\n");

            IEventLoopGroup bossGroup;
            IEventLoopGroup workGroup;
            bossGroup = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();

            X509Certificate2 tlsCertificate = null;
            if (ServerSettings.IsSsl)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(ServerSettings.X509Cert, "dotnetty.com.pfx"), "password");
            }
            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);

                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        pipeline.AddLast(new WebSocketServerHandler());
                    }));

                int port = ServerSettings.Port;
                IChannel bootstrapChannel = await bootstrap.BindAsync(IPAddress.Loopback, port);

                Console.WriteLine("Open your web browser and navigate to " 
                    + $"{(ServerSettings.IsSsl ? "https" : "http")}" 
                    + $"://127.0.0.1:{port}/");
                Console.WriteLine("Listening on "
                    + $"{(ServerSettings.IsSsl ? "wss" : "ws")}"
                    + $"://127.0.0.1:{port}/websocket");
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                workGroup.ShutdownGracefullyAsync().Wait();
                bossGroup.ShutdownGracefullyAsync().Wait();
            }
        }

        private static void Main() => RunServerAsync().Wait();
    }
}
