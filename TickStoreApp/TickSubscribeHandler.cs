using DotNetty.Transport.Channels;
using Konsole.Forms;
using System;

namespace TickStoreApp
{
    public class TickSubscribeHandler : SimpleChannelInboundHandler<string>
    {
        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            new Form(80, new ThickBoxStyle()).Write(new
            {
                StartAt = DateTime.Now.ToString(),
                LocalAddress = context.Channel.LocalAddress.ToString(),
                RemoteAddress = context.Channel.RemoteAddress.ToString()
            }, "Tick Subscribe"
             );
        }
        protected override void ChannelRead0(IChannelHandlerContext ctx, string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"local time:{time}{msg}");
        }
        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(e.StackTrace);
            contex.CloseAsync();
        }
    }
}
