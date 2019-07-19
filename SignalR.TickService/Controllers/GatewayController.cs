using Core;
using Newtonsoft.Json;
using SignalR.Tick.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace SignalR.Tick.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class GatewayController : Controller
    {
        [AllowCrossSiteJson]
        public ActionResult Schroder()
        {
            string path = Server.MapPath("/App_Data/Schroder/gateway-b.txt");
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
            string path = Server.MapPath("/App_Data/Schroder/gateway-b.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }



        [AllowCrossSiteJson]
        public ActionResult YingTou()
        {
            string path = Server.MapPath("/App_Data/YingTou/gateway-a.txt");
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
            string path = Server.MapPath("/App_Data/YingTou/gateway-a.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }


        /// <summary>
        /// http://ethereum66.com/#/home
        /// </summary>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public ActionResult Ethereum()
        {
            string path = Server.MapPath("/App_Data/Ethereum/gateway-c.txt");
            string text = System.IO.File.ReadAllText(path);
            List<GatewayItem> items = JsonConvert.DeserializeObject<List<GatewayItem>>(text);
            string json = JsonConvert.SerializeObject(items);
            string key = "aVEzbzVnN2c5YWJjZDB3WA==";
            string iv = "djEybmM1ZzdzOWFiY2Ruaw==";
            Tuple<string, string, string> result = AESUtils.Encrypt(json, key, iv);
            string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            var obj = new { data = result.Item1 };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowCrossSiteJson]
        public ActionResult XEthereum()
        {
            string path = Server.MapPath("/App_Data/Ethereum/gateway-c.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }

        /// <summary>
        /// http://yongfeng66.com/
        /// </summary>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public ActionResult YongFeng()
        {
            string path = Server.MapPath("/App_Data/YongFeng/gateway-d.txt");
            string text = System.IO.File.ReadAllText(path);
            List<GatewayItem> items = JsonConvert.DeserializeObject<List<GatewayItem>>(text);
            string json = JsonConvert.SerializeObject(items);
            string key = "aWMzbzVna2c5YXhjZDB3eg==";
            string iv = "dm0ybmM1bzdzOWFnY2RubA==";
            Tuple<string, string, string> result = AESUtils.Encrypt(json, key, iv);
            string t = AESUtils.Decrypt(result.Item1, result.Item2, result.Item3);
            var obj = new { data = result.Item1 };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [AllowCrossSiteJson]
        public ActionResult XYongFeng()
        {
            string path = Server.MapPath("/App_Data/YongFeng/gateway-d.txt");
            string text = System.IO.File.ReadAllText(path);
            text = text.Replace("\r\n", string.Empty);
            text = Regex.Replace(text, @"\s+", string.Empty);
            return Content(text);
        }

        public ActionResult Pay()
        {
            var obj = new PayGateway();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

    }
}