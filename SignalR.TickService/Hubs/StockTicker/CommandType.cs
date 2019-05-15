namespace SignalR.Tick.Hubs.StockTicker
{
    public enum CommandType
    {
        Ping = 1,
        Stop = 2,
        Subscribe = 3,  //订阅
        UnSubscribe = 4,//取消订阅
        Now = 5,
        Open = 6, //开市
        Close = 7,  //   休市
        Reset = 8
    }
}