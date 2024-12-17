namespace TelegramMonitor
{
    /// <summary>
    /// 表示从 API 返回的标准响应结构。
    /// 可用于通用的后端API接口返回解析。
    /// </summary>
    public class HttpReturn
    {
        /// <summary>
        /// HTTP状态码，例如200表示请求成功，400或500表示请求有误等。
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 数据列表。若请求成功通常包含与业务逻辑相关的数据（如广告内容字符串列表）。
        /// 若请求失败或无数据，可为空列表。
        /// </summary>
        public List<string> Data { get; set; } = new();

        /// <summary>
        /// 请求是否成功的逻辑标识符。当 Succeeded = true 时表明数据正常返回。
        /// 当为 false 时通常需要查看 Errors 字段以了解失败原因。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 若请求失败或发生错误，存储错误信息（例如异常消息、字段验证错误）。
        /// 若请求成功，通常为空。
        /// </summary>
        public string? Errors { get; set; }

        /// <summary>
        /// 扩展字段，用于存储额外的元信息或附加数据。
        /// 类型为 object，意味着可根据具体业务动态调整存放的数据结构和类型。
        /// 若无额外信息可为空。
        /// </summary>
        public object? Extras { get; set; }

        /// <summary>
        /// 时间戳（UTC时间的秒数或毫秒数），可用于标识返回数据的时间点，
        /// 以便于客户端缓存或分析请求延迟。
        /// </summary>
        public int Timestamp { get; set; }
    }
}