﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin;

namespace SignalR.Tick.Connections
{
    // Awesome name, it rhymes
    public class RedirectionConnection : PersistentConnection
    {
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
