using Core;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Konsole.Forms;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 行情广播服务
/// </summary>
namespace TickBoardcastApp
{
    internal class Program
    {
        static async Task RunServerAsync()
        {
            MultithreadEventLoopGroup bossGroup = new MultithreadEventLoopGroup();
            MultithreadEventLoopGroup workerGroup = new MultithreadEventLoopGroup();

            StringEncoder STRING_ENCODER = new StringEncoder(Encoding.UTF8);
            StringDecoder STRING_DECODER = new StringDecoder(Encoding.UTF8);
            TickBoardcastHandler SERVER_HANDLER = new TickBoardcastHandler();

            X509Certificate2 tlsCertificate = null;
            if (ServerSettings.IsSsl)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(ServerSettings.X509Cert, "dotnetty.com.pfx"), "password");
            }
            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler(LogLevel.INFO))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }

                        pipeline.AddLast(new DelimiterBasedFrameDecoder(8192, Delimiters.LineDelimiter()));
                        pipeline.AddLast(STRING_ENCODER, STRING_DECODER, SERVER_HANDLER);
                    }));

                IChannel bootstrapChannel = await bootstrap.BindAsync(IPAddress.Parse(ServerSettings.Bind), ServerSettings.Port);

                Console.ReadLine();
                // await bootstrapChannel.CloseAsync();
            }
            finally
            {
                //Task.WaitAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync());
            }
        }

        private static void Main()
        {
            new Form(60, new ThickBoxStyle()).Write(new
            {
                StartAt = DateTime.Now.ToString(),
                BindAddress = ServerSettings.Bind,
                Port = ServerSettings.Port
            }, "Live Tick Publish"
           );
            RunServerAsync().Wait();
        }
    }

}
