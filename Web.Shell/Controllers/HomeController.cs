﻿using System;
using System.Web.Mvc;

namespace Web.Shell.Controllers
{
    public class HomeController : Controller
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