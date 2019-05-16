using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace SignalR.Tick
{
    public class RawConnection : PersistentConnection
    {
        private static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public static ISubscriber RedisSub { get; } = Redis.GetSubscriber();
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> Clients = new ConcurrentDictionary<string, string>();

        public RawConnection()
        {
            ContractQuoteFull.Items.ForEach(item =>
            {
                RedisSub.Subscribe(item.Item2, (channel, value) =>
                {
                    //string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //string msg = $"{now}/{value}";
                    ContractQuoteFull data = JsonConvert.DeserializeObject<ContractQuoteFull>(value);
                    Groups.Send(channel, data);
                    Groups.Send("All", data);
                });
            });
        }
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Clients[connectionId] = now;
            _users[now] = connectionId;

            string clientIp = GetClientIP(request);

            string user = GetUser(connectionId);
            string msg = $"@{DateTime.Now } :  [{user}]  加入 from  [{clientIp}]";

            return Groups.Add(connectionId, "All").ContinueWith(x => Connection.Broadcast(msg)).Unwrap();
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            string user = GetUser(connectionId);

            return Connection.Broadcast(DateTime.Now + ": " + user + " 重连");
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            _users.TryRemove(connectionId, out string ignored);

            string suffix = stopCalled ? "cleanly" : "uncleanly";
            string msg = $"@{DateTime.Now}: User { GetUser(connectionId)}  断开: {suffix}";
            return Connection.Broadcast(msg);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            Message message = JsonConvert.DeserializeObject<Message>(data);

            switch (message.Type)
            {
                case MessageType.Broadcast:
                    Connection.Broadcast(new
                    {
                        type = MessageType.Broadcast,
                        from = GetUser(connectionId),
                        data = message.Value
                    });
                    break;
                case MessageType.BroadcastExceptMe:
                    Connection.Broadcast(new
                    {
                        type = MessageType.Broadcast,
                        from = GetUser(connectionId),
                        data = message.Value
                    },
                    connectionId);
                    break;
                case MessageType.SendToMe:
                    Connection.Send(connectionId, new
                    {
                        type = MessageType.SendToMe,
                        from = GetUser(connectionId),
                        data = message.Value
                    });
                    break;
                case MessageType.Join:
                    string name = message.Value;
                    Clients[connectionId] = name;
                    _users[name] = connectionId;
                    Connection.Send(connectionId, new
                    {
                        type = MessageType.Join,
                        data = message.Value
                    });
                    break;
                case MessageType.PrivateMessage:
                    var parts = message.Value.Split('|');
                    string user = parts[0];
                    string msg = parts[1];
                    string id = GetClient(user);
                    Connection.Send(id, new
                    {
                        from = GetUser(connectionId),
                        data = msg
                    });
                    break;
                case MessageType.AddToGroup:
                    Groups.Add(connectionId, message.Value);
                    break;
                case MessageType.RemoveFromGroup:
                    Groups.Remove(connectionId, message.Value);
                    break;
                case MessageType.SendToGroup:
                    var parts2 = message.Value.Split('|');
                    string groupName = parts2[0];
                    string val = parts2[1];
                    Groups.Send(groupName, val);
                    break;
                default:
                    break;
            }

            return base.OnReceived(request, connectionId, data);
        }
        protected override IList<string> OnRejoiningGroups(IRequest request, IList<string> groups, string connectionId)
        {
            return base.OnRejoiningGroups(request, groups, connectionId);
        }

        private string GetUser(string connectionId)
        {
            return !Clients.TryGetValue(connectionId, out string user) ? connectionId : user;
        }

        private string GetClient(string user)
        {
            return _users.TryGetValue(user, out string connectionId) ? connectionId : null;
        }

        private enum MessageType
        {
            SendToMe = 0,
            Broadcast = 1,
            Join = 2,
            PrivateMessage = 3,
            AddToGroup = 4,
            RemoveFromGroup = 5,
            SendToGroup = 6,
            BroadcastExceptMe = 7,
        }

        private class Message
        {
            public MessageType Type { get; set; }
            public string Value { get; set; }
        }

        private static string GetClientIP(IRequest request)
        {
            return Get<string>(request.Environment, "server.RemoteIpAddress");
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            return env.TryGetValue(key, out object value) ? (T)value : default(T);
        }
    }
}