namespace TelegramMonitor;

/// <summary>
/// 全局常量类，用于存储需要统一管理的常量值，如 API ID、API Hash、日志文件路径、API接口地址、轮询间隔等。
/// </summary>
public static class Constants
{
    public const int ApiId = 23319500;                                              //telegram apiId
    public const string ApiHash = "814ac0dd67f660119b9b990d514c9a47";               //telegram apihash
    public const string LogFilePath = "TelegramBot.log";                            //telegram日志保存位置
    public const string ApiUrl = "https://riniba.net/api/system/telegramMonitor";   //广告地址
    public const string KeywordsFile = "keywords.txt";                              //关键词地址

    // 间隔时间（秒），用于定时请求（这里为5分钟=300秒）
    public const int IntervalSeconds = 60 * 5;
}