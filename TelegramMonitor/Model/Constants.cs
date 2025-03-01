namespace TelegramMonitor;

/// <summary>
/// 全局常量配置类,管理系统配置参数
/// </summary>
public static class Constants
{
    /// <summary>
    /// Telegram API 配置
    /// </summary>
    public static class TelegramConfig
    {
        /// <summary>
        /// Telegram API ID
        /// </summary>
        public const int ApiId = 23319500;

        /// <summary>
        /// Telegram API Hash
        /// </summary>
        public const string ApiHash = "814ac0dd67f660119b9b990d514c9a47";
    }

    /// <summary>
    /// 文件路径配置
    /// </summary>
    public static class FilePaths
    {
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public const string LogFile = "TelegramBot.log";

        /// <summary>
        /// 关键词配置文件路径
        /// </summary>
        public const string KeywordsFile = "keywords.yaml";

        /// <summary>
        /// 代理配置文件路径
        /// </summary>
        public const string ProxyConfigFile = "proxyConfig.yaml";
    }

    /// <summary>
    /// 系统运行配置
    /// </summary>
    public static class SystemConfig
    {
        /// <summary>
        /// 轮询间隔(秒)
        /// </summary>
        public const int PollingIntervalSeconds = 300;

        /// <summary>
        ///广告Api
        /// </summary>
        public const string MonitorApi = "https://riniba.net/api/system/telegramMonitor";

        /// <summary>
        ///广告数据
        /// </summary>
        public static List<string> Advertisement = [];
    }

    /// <summary>
    /// 关键词配置列表
    /// </summary>
    public static List<KeywordConfig> Keywords { get; set; } = new();
}