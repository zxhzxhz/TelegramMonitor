namespace TelegramMonitor;

/// <summary>
/// 定义匹配到关键词或用户后的处理动作
/// </summary>
public enum KeywordAction
{
    /// <summary>
    /// 排除匹配的内容
    /// </summary>
    Exclude,

    /// <summary>
    /// 监控匹配的内容
    /// </summary>
    Monitor
}