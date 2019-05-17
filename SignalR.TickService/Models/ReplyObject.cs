using Newtonsoft.Json;
using System.Net;

namespace SignalR.Tick.Models
{
    public class ReplyObject
    {
        public ReplyObject()
        {

        }

        public static ReplyObject Empty { get => new ReplyObject(); }

        /// <summary>
        /// 请求编号（36位的UUID字符串，或其他唯一的字符串）
        /// </summary>
        public string RequestNo { get; set; }//: "202db6c0-1ab1-4e4c-a638-4278bfb1d48b",

        /// <summary>
        /// 指令ID
        /// </summary>
        public CommandType CmdType { get; set; }

        /// <summary>
        /// 状态码，200为正常，其他状态码为业务异常
        /// </summary>
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// 消息提示
        /// </summary>
        public string Message { get; set; }// "响应成功"

        public long ServerTime => System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public long ClientTime { get; set; }
        public long TimeElapsed => ClientTime - ServerTime;

        /// <summary>
        /// 耗时
        /// </summary>

        public static ReplyObject GetModuleInfo(string json)
        {
            ReplyObject result = JsonConvert.DeserializeObject<ReplyObject>(json);
            return result;
        }
        public override string ToString() => JsonConvert.SerializeObject(this);

    }
    /// <summary>
    /// tcp响应统一格式
    /// </summary>
    public class ReplyContent<T> : ReplyObject
    {
        public ReplyContent()
        {
        }
        public static ReplyContent<T> GetReplyContent(string json) => JsonConvert.DeserializeObject<ReplyContent<T>>(json);
        /// <summary>
        /// 响应实体对象
        /// </summary>
        public T Result { get; set; }
    }

}