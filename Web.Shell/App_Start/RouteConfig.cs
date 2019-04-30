using System.Web.Mvc;
using System.Web.Routing;

namespace Web.Shell
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            // /API/GC/1906/01/2019/04/03/17
            routes.MapRoute(
             name: "API",
             url: "API/{commodityNo}/{contractNo}/{type}/{year}/{month}/{day}/{hour}",
             defaults: new { controller = "Commodity", action = "Lines" },
             constraints: new { type = @"\d{2}", year = @"\d{4}", month = @"\d{2}", day = @"\d{2}", hour = @"\d{2}" }
             );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "App", action = "Main", id = UrlParameter.Optional }
            );
        }
    }
}
