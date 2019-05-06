using System;
using System.Collections.Generic;
using System.Net;

namespace SignalR.Tick.Hubs.Chat.ContentProviders
{
    public class ImageContentProvider : IContentProvider
    {
        private static readonly HashSet<string> _imageMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "image/png",
            "image/jpg",
            "image/jpeg",
            "image/bmp",
            "image/gif",
        };

        public string GetContent(HttpWebResponse response)
        {
            if (!string.IsNullOrEmpty(response.ContentType) &&
                _imageMimeTypes.Contains(response.ContentType))
            {
                return string.Format(@"<img src=""{0}"" />", response.ResponseUri);
            }
            return null;
        }
    }
}