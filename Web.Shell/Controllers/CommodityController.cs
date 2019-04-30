using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web.Mvc;

namespace Web.Shell.Controllers
{

    /// <summary>
    /// 
    /// </summary>
    public class CommodityController : BaseController
    {
        /// <summary>
        /// 114.67.236.124
        /// </summary>
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            Password = "03hx5DDDivYmbkTgDlFz",
            EndPoints = {
                new IPEndPoint(IPAddress.Parse("110.42.6.125"), 6379)
            },
            ChannelPrefix = "X"
        });

        /// <summary>
        /// /GC/1904/1/2019-5-4/1000/hash?=345678
        /// </summary>
        /// <param name="commodityNo"></param>
        /// <param name="contractNo"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Lines(string commodityNo, string contractNo, int type, int year, int month, int day, int hour)
        {
            Stopwatch watch = Stopwatch.StartNew();

            DateTime time = new DateTime(year, month, day, hour, 0, 0);

            string list_key = $"{commodityNo}:{contractNo}:{type}:{time.ToString("yyyy-MM-dd:HH")}";

            IDatabase rdb = redis.GetDatabase(1);

            RedisValue[] result = rdb.ListRange(list_key);

            string[] values = new string[result.Length];

            for (int i = 0; i < result.Length; i++)
            {

                values[i] = result[i];
            }
            watch.Stop();

            return Json(new
            {
                Request.UserHostAddress,
                Request.UserHostName,
                watch.Elapsed.TotalMilliseconds,
                Values = values
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult Database(int index)
        {
            IDatabase db = redis.GetDatabase(index);
            return Json(db, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Contact(int index)
        {
            IDatabase db = redis.GetDatabase(index);
            db.Execute("KEYS *", null);


            var items = db.ListRange("", 1, 90);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Keys(int index = 0)
        {
            IDatabase db = redis.GetDatabase(index);
            List<RedisValue> items = new List<RedisValue>();

            for (int i = 0; i < 100000; i++)
            {
                string key = $"foo{i}";
                RedisValue result = db.StringGet(key);
                items.Add(result);
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Subscriber()
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe("messages", (channel, message) =>
            {
                System.Diagnostics.Debug.WriteLine((string)message);
            });
            return Json(sub, JsonRequestBehavior.AllowGet);
        }
    }
}