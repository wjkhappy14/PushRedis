using System;
using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return Json(new { DateTime.Now }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Main()
        {
            return View();
        }
    }
}