using Newtonsoft.Json;

namespace SignalR.Tick.Models
{
    public class RequestCommand
    {
        /// <summary>
        /// I表示IOS, A表示Android, PC表示PC, H5表示移动端, D表示桌面程序
        /// </summary>
        public string RequestEndType { get; set; }

        /// <summary>
        /// 请求编号（36位的UUID字符串，或其他唯一的字符串）
        /// </summary>
        public string RequestNo { get; set; }

        /// <summary>
        /// 指令ID
        /// </summary>
        /// 
        public CommandType CmdType { get; set; }

        public long ClientTime { get; set; }
    }


    /// <summary>
    /// 请求指令
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class RequestCommand<T> : RequestCommand
    {
        public static RequestCommand<T> GetRequestCommand(string data)
        {
            return JsonConvert.DeserializeObject<RequestCommand<T>>(data);
        }
        /// <summary>
        /// 请求实体对象
        /// </summary>
        public T Data { get; set; } = default(T);
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}