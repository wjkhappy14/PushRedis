using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SignalR.Tick.Connections;
using SignalR.Tick.Models;
using StackExchange.Redis;

namespace SignalR.Tick
{
    public class RawConnection : AddGroupOnConnectedConnection
    {
        private static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public static ISubscriber RedisSub { get; } = Redis.GetSubscriber();
        private static readonly ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> Clients = new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<Tuple<string, string>, ContractQuoteFull> Topics = new ConcurrentDictionary<Tuple<string, string>, ContractQuoteFull>();

        public RawConnection()
        {
            ContractQuoteFull.Items.ForEach(topic => Topics.TryAdd(topic, ContractQuoteFull.Default(topic)));

            ReplyContent<object> reply = new ReplyContent<object>();
            ContractQuoteFull.Items.ForEach(item =>
            {
                RedisSub.Subscribe(item.Item2, (channel, value) =>
                {
                    //string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //string msg = $"{now}/{value}";
                    ContractQuoteFull contract = JsonConvert.DeserializeObject<ContractQuoteFull>(value);
                    reply.CmdType = CommandType.Publish;
                    reply.Result = contract;
                    Groups.Send(channel, reply);
                    Groups.Send("All", reply);
                });
            });
        }
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            ReplyContent<object> reply = new ReplyContent<object>();
            reply.CmdType = CommandType.Connected;

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            reply.Result = Topics.Values;

            Clients[connectionId] = now;
            Users[now] = connectionId;
            string clientIp = GetClientIP(request);
            string user = GetUser(connectionId);

            string msg = $"@{DateTime.Now } :  [{user}]  加入 from  [{clientIp}]";
            if (clientIp == "10.0.1.4")
            {
                msg += "You are  in  admin group!";
                reply.Message = msg;
                Groups.Add(connectionId, "admin").ContinueWith(x => Connection.Send(connectionId, reply));
            }
            reply.Message = msg;
            Connection.Send(connectionId, reply);

            reply.CmdType = CommandType.Join;
            reply.Result = clientIp;
            return Groups.Add(connectionId, "All").ContinueWith(x => Connection.Broadcast(reply)).Unwrap();
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            string user = GetUser(connectionId);
            ReplyContent<object> reply = new ReplyContent<object>();
            reply.Message = DateTime.Now + ": " + user + " 重连";
            return Connection.Broadcast(reply);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            Users.TryRemove(connectionId, out string ignored);
            ReplyContent<object> reply = new ReplyContent<object>();
            string suffix = stopCalled ? "cleanly" : "uncleanly";
            reply.Message = $"@{DateTime.Now}: User { GetUser(connectionId)}  断开: {suffix}";
            return Connection.Broadcast(reply);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            RequestCommand<string> requestCommand = RequestCommand<string>.GetRequestCommand(data);
            ReplyContent<object> reply = new ReplyContent<object>();
            reply.CmdType = CommandType.Publish;
            reply.RequestNo = requestCommand.RequestNo;
            reply.Result = new
            {
                type = CommandType.Broadcast,
                from = GetUser(connectionId),
                data = ""
            };

            switch (requestCommand.CmdType)
            {
                case CommandType.Broadcast:
                    Connection.Broadcast(reply);
                    break;
                case CommandType.BroadcastExceptMe:
                    Connection.Broadcast(reply, connectionId);
                    break;
                case CommandType.SendToMe:
                    Connection.Send(connectionId, reply);
                    break;
                case CommandType.Join:
                    string name = requestCommand.RequestNo;
                    Clients[connectionId] = name;
                    Users[name] = connectionId;

                    Connection.Send(connectionId, reply);

                    break;
                case CommandType.PrivateMessage:
                    string user = "";
                    string id = GetClient(user);

                    Connection.Send(id, reply);
                    break;
                case CommandType.AddToGroup:
                    Groups.Add(connectionId, "");
                    break;
                case CommandType.RemoveFromGroup:
                    Groups.Remove(connectionId, "");
                    break;
                case CommandType.SendToGroup:
                    string groupName = "";
                    string val = "";
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
            return Users.TryGetValue(user, out string connectionId) ? connectionId : null;
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