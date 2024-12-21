namespace TelegramMonitor
{
    /// <summary>
    /// API返回的标准响应结构
    /// </summary>
    public class HttpReturn
    {
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 业务数据列表
        /// </summary>
        public List<string> Data { get; set; } = new();

        /// <summary>
        /// 请求是否成功
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Errors { get; set; }

        /// <summary>
        /// 额外数据字段
        /// </summary>
        public object? Extras { get; set; }

        /// <summary>
        /// UTC时间戳
        /// </summary>
        public int Timestamp { get; set; }
    }
}