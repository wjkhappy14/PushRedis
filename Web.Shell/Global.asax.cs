using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web.Shell
{
    public class Global : HttpApplication
    {
      
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
          


        }
    }
}