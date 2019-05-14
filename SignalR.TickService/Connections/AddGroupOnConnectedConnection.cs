
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
  public  class AddGroupOnConnectedConnection : MyRejoinGroupsConnection
    {
        protected override async Task OnConnected(IRequest request, string connectionId)
        {
            string transport = request.QueryString["transport"];

            await Groups.Add(connectionId, "test");
            await Groups.Send("test", "hey");
            await Task.Delay(TimeSpan.FromSeconds(1));
            await PrintEnvironment("OnConnectedAsync", request, connectionId);
            await Groups.Add(connectionId, "test2");
            await base.OnConnected(request, connectionId);
        }

        protected override async Task OnReceived(IRequest request, string connectionId, string data)
        {
            string refererHeader = request.Headers[System.Net.HttpRequestHeader.Referer.ToString()];
            string testHeader = request.Headers["test-header"];
            string userAgentHeader = request.Headers["User-Agent"];

            ArraySegment<byte> jsonBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
            await Connection.Send(connectionId, jsonBytes);

            await Connection.Send(connectionId, new
            {
                refererHeader,
                testHeader,
                userAgentHeader
            });

            await Connection.Broadcast(data, connectionId);
            await Groups.Send("test2", "hey");
        }
        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            return Connection.Send(connectionId, request.Url.AbsolutePath.EndsWith("/reconnect"));
        }
        protected override bool AuthorizeRequest(IRequest request)
        {
            return request.User != null && request.User.Identity.IsAuthenticated;
        }
        private Task PrintEnvironment(string method, IRequest request, string connectionId)
        {
            return Connection.Broadcast(new
            {
                method,
                headers = request.Headers,
                query = request.QueryString,
                count = request.Environment.Count,
                owinKeys = request.Environment.Keys
            });
        }
        public override Task ProcessRequest(HostContext context)
        {
            string redirectWhen = "____Never____";

            if (!String.IsNullOrEmpty(context.Request.QueryString["redirectWhen"]))
            {
                redirectWhen = context.Request.QueryString["redirectWhen"];
            }

            OwinRequest owinRequest = new OwinRequest(context.Environment);

            if (owinRequest.Path.Value.Contains("/" + redirectWhen))
            {
                OwinResponse response = new OwinResponse(context.Environment);

                // Redirect to an invalid page
                response.Redirect("http://" + owinRequest.Host);

                //  return TaskAsyncHelper.Empty;
            }
            return base.ProcessRequest(context);
        }

    }
}
