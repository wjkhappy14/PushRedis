namespace SignalR.Tick.Models
{
    public enum CommandType
    {
        Connected = 0,
        Ping = 1,
        Stop = 2,
        Subscribe = 3,  //订阅
        UnSubscribe = 4,//取消订阅
        TimeNow = 5,
        Open = 6, //开市
        Close = 7,  //   休市
        Reset = 8,
        SendToMe = 10,
        Broadcast = 11,
        Join = 12,
        PrivateMessage = 13,
        AddToGroup = 14,
        RemoveFromGroup = 15,
        SendToGroup = 16,
        BroadcastExceptMe = 17,
        Publish=18
    }
}