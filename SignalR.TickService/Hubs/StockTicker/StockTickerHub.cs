using Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignalR.Tick.Hubs.StockTicker
{
    [HubName("stockTicker")]
    public class StockTickerHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ClientItem> _users = new ConcurrentDictionary<string, ClientItem>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userRooms = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, ChannelGroup> ChannelGroups = new ConcurrentDictionary<string, ChannelGroup>(StringComparer.OrdinalIgnoreCase);

        public bool Join(string connectionId, string groupName)
        {
            Groups.Add(connectionId, groupName).Wait();
            dynamic g = Clients.Group(groupName);

            ClientItem user = _users.Values.FirstOrDefault(u => u.Id == groupName);

            if (user != null)
            {
                // Update the users's client id mapping
                user.ConnectionId = Context.ConnectionId;

                // Set some client state
                Clients.Caller.id = user.Id;
                Clients.Caller.name = user.Name;
                Clients.Caller.hash = user.Hash;

                // Leave all rooms
                HashSet<string> rooms;
                if (_userRooms.TryGetValue(user.Name, out rooms))
                {
                    foreach (var room in rooms)
                    {
                        Clients.Group(room).leave(user);
                        var chatRoom = ChannelGroups[room];
                        chatRoom.Users.Remove(user.Name);
                    }
                }

                _userRooms[user.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Add this user to the list of users
                Clients.Caller.addUser(user);
                return true;
            }

            return false;
        }

        public void SendCmd(Command cmd)
        {
            string content = cmd.Text.Replace("<", "&lt;").Replace(">", "&gt;");

            if (!TryHandleCommand(cmd))
            {
                string roomName = Clients.Caller.room;
                string name = Clients.Caller.name;

                EnsureUserAndRoom();

                string messageText = Transform(content, out HashSet<string> links);

                ChannelGroups[roomName].Messages.Add(cmd);

                Clients.Group(roomName).addMessage(cmd.Id, cmd.User, cmd.Text);

                if (links.Any())
                {
                    // REVIEW: is this safe to do? We're holding on to this instance 
                    // when this should really be a fire and forget.
                    Task<string>[] contentTasks = links.Select(ExtractContent).ToArray();
                    Task.Factory.ContinueWhenAll(contentTasks, tasks =>
                    {
                        foreach (Task<string> task in tasks)
                        {
                            if (task.IsFaulted)
                            {
                                Trace.TraceError(task.Exception.GetBaseException().Message);
                                continue;
                            }

                            if (string.IsNullOrEmpty(task.Result))
                            {
                                continue;
                            }

                            // Try to get content from each url we're resolved in the query
                            string extractedContent = "<p>" + task.Result + "</p>";

                            // If we did get something, update the message and notify all clients
                            cmd.Text += extractedContent;

                            Clients.Group(roomName).addMessageContent(cmd.Id, extractedContent);
                        }
                    });
                }
            }
        }

        public long GetUnixTimeMilliseconds() => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public string GetTimeNow() => DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF");

        public List<Tuple<string, string>> GetSymbols() => ContractQuoteFull.Items;

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            ClientItem user = _users.Values.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                _users.TryRemove(user.Name, out ClientItem ignoredUser);

                // Leave all rooms
                if (_userRooms.TryGetValue(user.Name, out HashSet<string> rooms))
                {
                    foreach (var room in rooms)
                    {
                        Clients.Group(room).leave(user);
                        var chatRoom = ChannelGroups[room];
                        chatRoom.Users.Remove(user.Name);
                    }
                }

                _userRooms.TryRemove(user.Name, out HashSet<string> ignoredRoom);
            }

            return null;
        }

        public IGroupManager GetGroups() => Groups;
        public IEnumerable<ClientItem> GeClients()
        {
            string room = Clients.Caller.room;

            if (string.IsNullOrEmpty(room))
            {
                return Enumerable.Empty<ClientItem>();
            }

            return from name in ChannelGroups[room].Users
                   select _users[name];
        }

        private string GetMD5Hash(string name)
        {
            return string.Join("", MD5.Create()
                         .ComputeHash(Encoding.Default.GetBytes(name))
                         .Select(b => b.ToString("x2")));
        }

        private bool TryHandleCommand(Command cmd)
        {
            string room = Clients.Caller.room;
            string name = Clients.Caller.name;


            string[] parts = cmd.Text.Substring(1).Split(' ');
            string commandName = parts[0];

            if (commandName.Equals("nick", StringComparison.OrdinalIgnoreCase))
            {
                string newUserName = String.Join(" ", parts.Skip(1));

                if (string.IsNullOrEmpty(newUserName))
                {
                    throw new InvalidOperationException("No username specified!");
                }

                if (newUserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("That's already your username...");
                }

                if (!_users.ContainsKey(newUserName))
                {
                    if (string.IsNullOrEmpty(name) || !_users.ContainsKey(name))
                    {
                        AddUser(newUserName);
                    }
                    else
                    {
                        ClientItem oldUser = _users[name];
                        ClientItem newUser = new ClientItem
                        {
                            Name = newUserName,
                            Hash = GetMD5Hash(newUserName),
                            Id = oldUser.Id,
                            ConnectionId = oldUser.ConnectionId
                        };

                        _users[newUserName] = newUser;
                        _userRooms[newUserName] = new HashSet<string>(_userRooms[name]);

                        if (_userRooms[name].Any())
                        {
                            foreach (string r in _userRooms[name])
                            {
                                ChannelGroups[r].Users.Remove(name);
                                ChannelGroups[r].Users.Add(newUserName);
                                Clients.Group(r).changeUserName(oldUser, newUser);
                            }
                        }
                        _userRooms.TryRemove(name, out HashSet<string> ignoredRoom);
                        _users.TryRemove(name, out ClientItem ignoredUser);

                        Clients.Caller.hash = newUser.Hash;
                        Clients.Caller.name = newUser.Name;

                        Clients.Caller.changeUserName(oldUser, newUser);
                    }
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Username '{0}' is already taken!", newUserName));
                }

                return true;
            }
            else
            {
                EnsureUser();
                if (commandName.Equals("rooms", StringComparison.OrdinalIgnoreCase))
                {
                    var rooms = ChannelGroups.Select(r => new
                    {
                        Name = r.Key,
                        Count = r.Value.Users.Count
                    });

                    Clients.Caller.showRooms(rooms);

                    return true;
                }
                else if (commandName.Equals("join", StringComparison.OrdinalIgnoreCase))
                {
                    if (parts.Length == 1)
                    {
                        throw new InvalidOperationException("Join which room?");
                    }

                    // Only support one room at a time for now

                    var newRoom = parts[1];
                    ChannelGroup chatRoom;
                    // Create the room if it doesn't exist
                    if (!ChannelGroups.TryGetValue(newRoom, out chatRoom))
                    {
                        chatRoom = new ChannelGroup();
                        ChannelGroups.TryAdd(newRoom, chatRoom);
                    }

                    // Remove the old room
                    if (!String.IsNullOrEmpty(room))
                    {
                        _userRooms[name].Remove(room);
                        ChannelGroups[room].Users.Remove(name);

                        Clients.Group(room).leave(_users[name]);
                        Groups.Remove(Context.ConnectionId, room);
                    }

                    _userRooms[name].Add(newRoom);
                    if (!chatRoom.Users.Add(name))
                    {
                        throw new InvalidOperationException("You're already in that room!");
                    }

                    Clients.Group(newRoom).addUser(_users[name]);

                    // Set the room on the caller
                    Clients.Caller.room = newRoom;

                    Groups.Add(Context.ConnectionId, newRoom);

                    Clients.Caller.refreshRoom(newRoom);

                    return true;
                }
                else if (commandName.Equals("msg", StringComparison.OrdinalIgnoreCase))
                {
                    if (_users.Count == 1)
                    {
                        throw new InvalidOperationException("You're the only person in here...");
                    }

                    if (parts.Length < 2)
                    {
                        throw new InvalidOperationException("Who are you trying send a private message to?");
                    }

                    string to = parts[1];
                    if (to.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("You can't private message yourself!");
                    }

                    if (!_users.ContainsKey(to))
                    {
                        throw new InvalidOperationException(String.Format("Couldn't find any user named '{0}'.", to));
                    }

                    string messageText = String.Join(" ", parts.Skip(2)).Trim();

                    if (string.IsNullOrEmpty(messageText))
                    {
                        throw new InvalidOperationException(String.Format("What did you want to say to '{0}'.", to));
                    }

                    string recipientId = _users[to].ConnectionId;
                    // Send a message to the sender and the sendee                        
                    Clients.Group(recipientId).sendPrivateMessage(name, to, messageText);
                    Clients.Caller.sendPrivateMessage(name, to, messageText);

                    return true;
                }
                else
                {
                    EnsureUserAndRoom();
                    if (commandName.Equals("me", StringComparison.OrdinalIgnoreCase))
                    {
                        if (parts.Length == 1)
                        {
                            throw new InvalidProgramException("You what?");
                        }
                        string content = String.Join(" ", parts.Skip(1));

                        Clients.Group(room).sendMeMessage(name, content);
                        return true;
                    }
                    else if (commandName.Equals("leave", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ChannelGroups.TryGetValue(room, out ChannelGroup chatRoom))
                        {
                            chatRoom.Users.Remove(name);
                            _userRooms[name].Remove(room);

                            Clients.Group(room).leave(_users[name]);
                        }

                        Groups.Remove(Context.ConnectionId, room);

                        Clients.Caller.room = null;

                        return true;
                    }

                    throw new InvalidOperationException(String.Format("'{0}' is not a valid command.", parts[0]));
                }
            }
            return false;
        }

        private ClientItem AddUser(string newUserName)
        {
            ClientItem user = new ClientItem(newUserName, GetMD5Hash(newUserName));
            user.ConnectionId = Context.ConnectionId;
            _users[newUserName] = user;
            _userRooms[newUserName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Clients.Caller.name = user.Name;
            Clients.Caller.hash = user.Hash;
            Clients.Caller.id = user.Id;

            Clients.Caller.addUser(user);

            return user;
        }

        private void EnsureUserAndRoom()
        {
            EnsureUser();

            // TODO: Restore when groups work
            string room = Clients.Caller.room;
            string name = Clients.Caller.name;

            if (string.IsNullOrEmpty(room) || !ChannelGroups.ContainsKey(room))
            {
                throw new InvalidOperationException("Use '/join room' to join a room.");
            }

            if (!_userRooms.TryGetValue(name, out HashSet<string> rooms) || !rooms.Contains(room))
            {
                throw new InvalidOperationException(String.Format("You're not in '{0}'. Use '/join {0}' to join it.", room));
            }
        }

        private void EnsureUser()
        {
            string name = Clients.Caller.name;
            if (string.IsNullOrEmpty(name) || !_users.ContainsKey(name))
            {
                throw new InvalidOperationException("You don't have a name. Pick a name using '/nick nickname'.");
            }
        }

        private string Transform(string message, out HashSet<string> extractedUrls)
        {
            const string urlPattern = @"((https?|ftp)://|www\.)[\w]+(.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";

            HashSet<string> urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            message = Regex.Replace(message, urlPattern, m =>
            {
                string httpPortion = String.Empty;
                if (!m.Value.Contains("://"))
                {
                    httpPortion = "http://";
                }

                string url = httpPortion + m.Value;

                urls.Add(url);

                return string.Format(CultureInfo.InvariantCulture,
                                     "<a rel=\"nofollow external\" target=\"_blank\" href=\"{0}\" title=\"{1}\">{1}</a>",
                                     url, m.Value);
            });

            extractedUrls = urls;
            return message;
        }

        private Task<string> ExtractContent(string url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            var requestTask = Task.Factory.FromAsync((cb, state) => request.BeginGetResponse(cb, state), ar => request.EndGetResponse(ar), null);
            return requestTask.ContinueWith(task => ExtractContent((HttpWebResponse)task.Result));
        }

        private string ExtractContent(HttpWebResponse response)
        {
            return response.CharacterSet;
        }

        [Serializable]
        public class Command
        {
            public string Id { get; set; }
            public string User { get; set; }
            public string Text { get; set; }
            public Command()
            {

            }
        }

        [Serializable]
        public class ClientItem
        {
            public string ConnectionId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Hash { get; set; }

            public ClientItem()
            {
            }

            public ClientItem(string name, string hash)
            {
                Name = name;
                Hash = hash;
                Id = Guid.NewGuid().ToString("d");
            }
        }

        public class ChannelGroup
        {
            public List<Command> Messages { get; set; }
            public HashSet<string> Users { get; set; }

            public ChannelGroup()
            {
                Messages = new List<Command>();
                Users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private StockTicker StockTicker { get; }

        public StockTickerHub() : this(StockTicker.Instance)
        {

        }

        public StockTickerHub(StockTicker stockTicker)
        {
            StockTicker = stockTicker;
        }

        public IEnumerable<ContractQuoteFull> GetAllStocks()
        {
            return StockTicker.GetAllStocks();
        }

        public string GetMarketState()
        {
            return StockTicker.MarketState.ToString();
        }

        /// <summary>
        /// 开市
        /// </summary>
        public void OpenMarket()
        {
            StockTicker.OpenMarket();
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="item"></param>
        public void Subscribe(string item)
        {


        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="item"></param>
        public void UnSubscribe(string item)
        {

        }

        /// <summary>
        /// 当前时间
        /// </summary>
        /// <returns></returns>
        public string Now()
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return now;
        }

        /// <summary>
        /// 休市
        /// </summary>
        public void CloseMarket()
        {
            StockTicker.CloseMarket();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            StockTicker.Reset();
        }
    }
}