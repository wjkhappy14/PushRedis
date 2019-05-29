using Core;
using SignalR.Tick.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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

        public JsonResult RSA(string content)
        {
            Tuple<RSAParameters, byte[]> result = RSAUtils.Encrypt(content);
            string data = Convert.ToBase64String(result.Item2);
            string t = RSAUtils.Decrypt(data, result.Item1);
            return Json(new { result.Item1, data, t }, JsonRequestBehavior.AllowGet);
        }
    }
}