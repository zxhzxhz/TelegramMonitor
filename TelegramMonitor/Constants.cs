namespace TelegramMonitor;

/// <summary>
/// 全局常量配置类，管理系统配置参数
/// </summary>
public static class Constants
{
    // Telegram凭据
    public const int TELEGRAM_API_ID = 23319500;
    public const string TELEGRAM_API_HASH = "814ac0dd67f660119b9b990d514c9a47";

    // 文件配置
    public const string LOG_FILE_PATH = "TelegramBot.log";
    public const string KEYWORDS_FILE_PATH = "keywords.txt";

    // API地址
    public const string MONITOR_API_ENDPOINT = "https://riniba.net/api/system/telegramMonitor";

    // 轮询间隔(秒)
    public const int POLLING_INTERVAL_SECONDS = 300; // 5分钟
}