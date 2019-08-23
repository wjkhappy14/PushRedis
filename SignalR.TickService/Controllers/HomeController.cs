using Core;
using Newtonsoft.Json;
using SignalR.Tick.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Main()
        {
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

       

        public ActionResult Info(int t = 1)
        {
            List<GatewayItem> items = t == 1 ? GatewayItem.A : GatewayItem.B;
            string json = JsonConvert.SerializeObject(items);
            return Content(json);
        }

        public JsonResult DES(string content)
        {
            Tuple<string, string, string> result = DESUtils.Encrypt(content);

            string t = DESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            return Json(new { result, t }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AES(int t = 1)
        {
            string key = t == 1 ? "1Q345g789abcdEwX" : "01d34u678pABCyEF";
            string iv = t == 1 ? "012n45g789abcdEk" : "01x34d678pA9CzEb";
            string filename = t == 1 ? "/App_Data/gateway-a.txt" : "/App_Data/gateway-b.txt";

            string path = Server.MapPath(filename);
            string content = System.IO.File.ReadAllText(path);

            Tuple<string, string, string> result = AESUtils.Encrypt(content, key, iv);

            string dec = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);

            var obj = new
            {
                Cotent = content,
                Base64AESText = result.Item1,
                Key = key,
                Base64Key = result.Item2,
                IV = iv,
                Base64IV = result.Item3
            };
            string json = JsonConvert.SerializeObject(obj);
            return Content(result.Item1);
        }

        public JsonResult RSA(string content)
        {
            Tuple<RSAParameters, byte[]> result = RSAUtils.Encrypt(content);
            string data = Convert.ToBase64String(result.Item2);
            string t = RSAUtils.Decrypt(data, result.Item1);
            return Json(new { result.Item1, data, t }, JsonRequestBehavior.AllowGet);
        }

        [AllowCrossSiteJson]
        public ActionResult Gateway()
        {
            string path = Server.MapPath("/App_Data/gateway-a-test.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            //key: 0123456789ABCDEF
            //IV: 0123456789abcdef
            Tuple<string, string, string> result = AESUtils.Encrypt(text, "MDEyMzQ1Njc4OUFCQ0RFRg==", "MDEyMzQ1Njc4OWFiY2RlZg==");
            // string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            var obj = new { data = result.Item1 };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        [AllowCrossSiteJson]
        public ActionResult XGateway()
        {
            string path = Server.MapPath("/App_Data/gateway-a.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }


        public ActionResult ECDiffieHellman()
        {
            ECDiffieHellmanUtils.X();
            return View();
        }

        public ActionResult Demo()
        {
            string path = Server.MapPath("/App_Data/gateway.txt");
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<GatewayItem> items = JsonConvert.DeserializeObject<List<GatewayItem>>(json);
                var obj = new
                {
                    standby = new List<string>() {
                        "http://42.51.45.70/gateway.txt",
                        "http://65.52.173.5/home/xgateway",
                        "http://65.52.173.5/home/gateway" },
                    entry = items
                };
                json = JsonConvert.SerializeObject(obj);
                return Content(json);
            }
        }
        public ActionResult WebSocket()
        {
            return View();
        }

        [AllowCrossSiteJson]
        public ActionResult Ping()
        {
            string base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            return Content(base64);
        }
    }
}