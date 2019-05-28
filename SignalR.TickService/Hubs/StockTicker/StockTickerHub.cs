﻿using Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.Tick.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Tick.Hubs.StockTicker
{
    [HubName("stockTicker")]
    public class StockTickerHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ClientItem> ClientItems = new ConcurrentDictionary<string, ClientItem>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userRooms = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, ChannelGroup> ChannelGroups = new ConcurrentDictionary<string, ChannelGroup>(StringComparer.OrdinalIgnoreCase);

        public bool Join(string connectionId, string groupName)
        {
            Groups.Add(connectionId, groupName).Wait();
            dynamic g = Clients.Group(groupName);
            ClientItem user = ClientItems.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
            if (user != null)
            {
                // Update the users's client id mapping
                user.ConnectionId = Context.ConnectionId;

                // Set some client state
                Clients.Caller.id = user.Id;
                Clients.Caller.name = user.Name;
                Clients.Caller.hash = user.Hash;

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
                _userRooms[user.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Add this user to the list of users
                Clients.Caller.addUser(user);
                return true;
            }
            return false;
        }

        public void SendCmd(Command cmd)
        {
            dynamic caller = Clients.Caller;
            int id = 0;
            Clients.Caller.id = Interlocked.Increment(ref id);

            if (cmd.CmdType == CommandType.Subscribe)
            {
                Groups.Add(Context.ConnectionId, cmd.Text);
            }
            else if (cmd.CmdType == CommandType.UnSubscribe)
            {
                Groups.Remove(Context.ConnectionId, cmd.Text);
            }
            else if (cmd.CmdType == CommandType.TimeNow)
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Clients.Client(Context.ConnectionId);
            }
            else if (cmd.CmdType == CommandType.Ping)
            {
                Clients.Caller.pong();
            }
            string content = cmd.Text.Replace("<", "&lt;").Replace(">", "&gt;");

            StockTicker.OpenMarket();
            StockTicker.CloseMarket();
            StockTicker.Reset();

            dynamic g = Clients.Group(cmd.Text);
            cmd.Text += $@"{UnixTimeMilliseconds}@{cmd.User}";
            g.addMessage(cmd.Id, cmd.User, cmd.Text);

            if (Clients != null)
            {
                g.addMessageContent(cmd.Id, cmd.Text);
            }
        }
        private long UnixTimeMilliseconds => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public string GetTimeNow() => DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFF");

        public List<Tuple<string, string>> GetSymbols() => ContractQuoteFull.Items;

        public override Task OnReconnected()
        {
            return Clients.All.reconnected(Context.ConnectionId, DateTime.Now.ToString());
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            ClientItems.TryRemove(Context.ConnectionId, out ClientItem item);
            return Groups.Remove(Context.ConnectionId, "");

        }
        public override Task OnConnected()
        {
            dynamic all = Clients.All;
            ClientItems.TryAdd(Context.ConnectionId, new ClientItem()
            {
                ConnectionId = Context.ConnectionId,
                Hash = GetMD5Hash(Context.ConnectionId)
            });
            return base.OnConnected();
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
                   select ClientItems[name];
        }

        private string GetMD5Hash(string connectionId)
        {
            return string.Join("", MD5.Create()
                         .ComputeHash(Encoding.Default.GetBytes(connectionId))
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

                if (!ClientItems.ContainsKey(newUserName))
                {
                    if (string.IsNullOrEmpty(name) || !ClientItems.ContainsKey(name))
                    {
                        AddUser(newUserName);
                    }
                    else
                    {
                        ClientItem oldUser = ClientItems[name];
                        ClientItem newUser = new ClientItem
                        {
                            Name = newUserName,
                            Hash = GetMD5Hash(newUserName),
                            Id = oldUser.Id,
                            ConnectionId = oldUser.ConnectionId
                        };

                        ClientItems[newUserName] = newUser;
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
                        ClientItems.TryRemove(name, out ClientItem ignoredUser);

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

                        Clients.Group(room).leave(ClientItems[name]);
                        Groups.Remove(Context.ConnectionId, room);
                    }

                    _userRooms[name].Add(newRoom);
                    if (!chatRoom.Users.Add(name))
                    {
                        throw new InvalidOperationException("You're already in that room!");
                    }
                    Clients.Group(newRoom).addUser(ClientItems[name]);

                    // Set the room on the caller
                    Clients.Caller.room = newRoom;

                    Groups.Add(Context.ConnectionId, newRoom);

                    Clients.Caller.refreshRoom(newRoom);

                    return true;
                }
                else if (commandName.Equals("msg", StringComparison.OrdinalIgnoreCase))
                {
                    if (ClientItems.Count == 1)
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

                    if (!ClientItems.ContainsKey(to))
                    {
                        throw new InvalidOperationException(String.Format("Couldn't find any user named '{0}'.", to));
                    }

                    string messageText = String.Join(" ", parts.Skip(2)).Trim();

                    if (string.IsNullOrEmpty(messageText))
                    {
                        throw new InvalidOperationException(String.Format("What did you want to say to '{0}'.", to));
                    }

                    string recipientId = ClientItems[to].ConnectionId;
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

                            Clients.Group(room).leave(ClientItems[name]);
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
            ClientItems[newUserName] = user;
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
            if (string.IsNullOrEmpty(name) || !ClientItems.ContainsKey(name))
            {
                throw new InvalidOperationException("You don't have a name. Pick a name using '/nick nickname'.");
            }
        }
        private Task<string> ExtractContent(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            Task<WebResponse> requestTask = Task.Factory.FromAsync((cb, state) => request.BeginGetResponse(cb, state), ar => request.EndGetResponse(ar), null);
            return requestTask.ContinueWith(task => HttpContent((HttpWebResponse)task.Result));
        }

        private string HttpContent(HttpWebResponse response)
        {
            return response.CharacterSet;
        }
        private StockTicker StockTicker { get; }

        public StockTickerHub() : this(StockTicker.Instance)
        {
        }

        private StockTickerHub(StockTicker stockTicker)
        {
            StockTicker = stockTicker;
        }

        public IEnumerable<ContractQuoteFull> GetAllStocks()
        {
            return StockTicker.GetAllStocks();
        }
    }
}