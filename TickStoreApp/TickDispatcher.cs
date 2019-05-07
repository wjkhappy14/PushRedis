using StackExchange.Redis;
using System.Net;
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Core;

namespace TickStoreApp
{
    public class TickDispatcher
    {
        public static MySqlConnection connection = new MySqlConnection("Server=107.180.41.46; database=wallhaven; UID=nginx; password=nginx; SSLMode=none");
        private static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();

        static private void Subscribe(string channelName)
        {
            ISubscriber sub = Redis.GetSubscriber();
            sub.Subscribe(channelName, (channel, message) =>
            {
                long id = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Console.WriteLine((string)message);

                string item = message.ToString();

                string[] values = item.Split(',');
                string timestamp = values[0];
                string qlastprice = values[1];
                string qlastqty = values[2];

                string insert = $@"
INSERT INTO `wallhaven`.`tick_millisecond`
(`id`,
`qlastqty`,
`qlastprice`,
`timestamp`)
VALUES
({id},
'{qlastqty}',
'{qlastprice}',
'{timestamp}');
";
                int x = MySqlHelper.ExecuteNonQuery(connection, insert);
            });
        }

        public static void Start(List<string> channels)
        {
            connection.Open();
            Subscribe("HKEX:MHI:1904");
            Subscribe("SGX:CN:1904");

            Console.ReadLine();
        }
    }
}

