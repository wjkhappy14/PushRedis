using Core;
using Newtonsoft.Json;
using SignalR.Tick.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class TestController : Controller
    {
        [AllowCrossSiteJson]
        public ActionResult Schroder()
        {
            string path = Server.MapPath("/App_Data/gateway-b-test.txt");
            string text = System.IO.File.ReadAllText(path);
            List<GatewayItem> items = JsonConvert.DeserializeObject<List<GatewayItem>>(text);
            string json = JsonConvert.SerializeObject(items);
            string key = "MDFkMzR1Njc4cEFCQ3lFRg==";
            string iv = "MDF4MzRkNjc4cEE5Q3pFYg==";
            Tuple<string, string, string> result = AESUtils.Encrypt(json, key, iv);
            // string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            var obj = new { data = result.Item1 };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowCrossSiteJson]
        public ActionResult XSchroder()
        {
            string path = Server.MapPath("/App_Data/gateway-b-test.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }



        [AllowCrossSiteJson]
        public ActionResult YingTou()
        {
            string path = Server.MapPath("/App_Data/gateway-a-test.txt");
            string text = System.IO.File.ReadAllText(path);
            List<GatewayItem> items = JsonConvert.DeserializeObject<List<GatewayItem>>(text);
            string json = JsonConvert.SerializeObject(items);
            string key = "MVEzNDVnNzg5YWJjZEV3WA==";
            string iv = "MDEybjQ1Zzc4OWFiY2RFaw==";
            Tuple<string, string, string> result = AESUtils.Encrypt(json, key, iv);
            // string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            var obj = new { data = result.Item1 };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowCrossSiteJson]
        public ActionResult XYingTou()
        {
            string path = Server.MapPath("/App_Data/gateway-a-test.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }

    }
}