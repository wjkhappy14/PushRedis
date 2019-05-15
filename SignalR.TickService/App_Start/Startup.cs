
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SignalR.Tick;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.AspNet.SignalR;
using SignalR.Tick.Connections;

[assembly: OwinStartup(typeof(Startup))]

namespace SignalR.Tick
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR<SendingConnection>("/sending-connection");
            app.MapSignalR<TestConnection>("/test-connection");
            app.MapSignalR<RawConnection>("/raw-connection");
            app.MapSignalR<StreamingConnection>("/streaming-connection");

            app.Use(typeof(ClaimsMiddleware));
            ConfigureSignalR(GlobalHost.DependencyResolver, GlobalHost.HubPipeline);
            //GlobalHost.DependencyResolver.UseRedis("110.42.6.125", 6379, "03hx5DDDivYmbkTgDlFz", "signalR");
            //跨域
            app.Map("/cors", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.MapSignalR<RawConnection>("/raw-connection");
                map.MapSignalR();
            });

            app.Map("/cookieauth", map =>
            {
                CookieAuthenticationOptions options = new CookieAuthenticationOptions()
                {
                    AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                    LoginPath = CookieAuthenticationDefaults.LoginPath,
                    LogoutPath = CookieAuthenticationDefaults.LogoutPath,
                };

                map.UseCookieAuthentication(options);

                map.Use(async (context, next) =>
                {
                    if (context.Request.Path.Value.Contains(options.LoginPath.Value))
                    {
                        if (context.Request.Method == "POST")
                        {
                            IFormCollection form = await context.Request.ReadFormAsync();
                            string userName = form["UserName"];
                            string password = form["Password"];

                            ClaimsIdentity identity = new ClaimsIdentity(options.AuthenticationType);
                            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                            context.Authentication.SignIn(identity);
                        }
                    }
                    else
                    {
                        await next();
                    }
                });

                map.MapSignalR<AddGroupOnConnectedConnection>("/echo");
                map.MapSignalR();
            });

            HubConfiguration config = new HubConfiguration()
            {
                EnableDetailedErrors = true
            };

            app.MapSignalR(config);

            BackgroundThread.Start();
        }

        private class ClaimsMiddleware : OwinMiddleware
        {
            public ClaimsMiddleware(OwinMiddleware next)
                : base(next)
            {
            }

            public override Task Invoke(IOwinContext context)
            {
                string username = context.Request.Headers.Get("username");

                if (string.IsNullOrEmpty(username))
                {
                    string authenticated = username == "john" ? "true" : "false";

                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Authentication, authenticated)
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
                    context.Request.User = new ClaimsPrincipal(claimsIdentity);
                }

                return Next.Invoke(context);
            }
        }
    }
}