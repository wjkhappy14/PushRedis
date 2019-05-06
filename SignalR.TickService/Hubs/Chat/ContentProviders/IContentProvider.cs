using System.Net;

namespace SignalR.Tick.Hubs.Chat
{
    public interface IContentProvider
    {
        string GetContent(HttpWebResponse response);
    }
}