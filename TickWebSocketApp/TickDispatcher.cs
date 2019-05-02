using StackExchange.Redis;
using System.Net;

namespace TickWebSocketApp
{
    internal class TickDispatcher
    {

        private static ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            Password = "03hx5DDDivYmbkTgDlFz",
            EndPoints = {
                new IPEndPoint(IPAddress.Parse("110.42.6.125"), 6379)
            }
            // ChannelPrefix = "X"
        });
    }
}
