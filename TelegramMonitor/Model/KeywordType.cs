namespace TelegramMonitor;

/// <summary>
/// 关键词匹配类型
/// </summary>
public enum KeywordType
{
    /// <summary>
    /// 全字匹配
    /// </summary>
    [Description("全字匹配")]
    FullWord,

    /// <summary>
    /// 包含指定文本
    /// </summary>
    [Description("包含指定文本")]
    Contains,

    /// <summary>
    /// 使用正则表达式匹配
    /// </summary>
    [Description("使用正则表达式匹配")]
    Regex,

    /// <summary>
    /// 模糊匹配多个关键词(以?分隔)
    /// </summary>
    [Description("模糊匹配多个关键词(以?分隔)")]
    Fuzzy,

    /// <summary>
    /// 匹配特定用户
    /// </summary>
    [Description("匹配特定用户")]
    User
}