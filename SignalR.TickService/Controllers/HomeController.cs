﻿using Core;
using SignalR.Tick.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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

        [HttpGet]
        public JavaScriptResult Tmpl()
        {
            ElementTag tag = ElementTag.Create();
            // string js = JsonConvert.SerializeObject(tag);
            string js = "window.TPL={}";
            return JavaScript(js);
        }

        public JsonResult Info(string contractNo)
        {
            Dictionary<string, string> data = ContractQuoteFull.Fake(contractNo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DES(string content)
        {
            Tuple<string, string, string> result = DESUtils.Encrypt(content);

            string t = DESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            return Json(new { result, t }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AES(string content)
        {
            Tuple<string, string, string> result = AESUtils.Encrypt(content);

            string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            return Json(new { result, t }, JsonRequestBehavior.AllowGet);
        }
    }
}