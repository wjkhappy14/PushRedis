using System;
using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class BaseController : Controller
    {

        private DateTime Now => DateTime.Now;
        public BaseController()
        {


        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
    }
}