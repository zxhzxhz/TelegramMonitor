namespace TelegramMonitor
{
    /// <summary>
    /// API 返回的标准响应结构
    /// </summary>
    public class HttpReturn
    {
        /// <summary>
        /// 状态代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回结果列表
        /// </summary>
        public List<string> Result { get; set; }

        /// <summary>
        /// 额外信息
        /// </summary>
        public object Extras { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Time { get; set; }
    }
}