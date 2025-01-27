namespace TelegramMonitor;

/// <summary>
/// 日志输出功能扩展类
/// </summary>
public static class LogExtensions
{
    /// <summary>
    /// 显示程序欢迎界面
    /// </summary>
    public static void Logo()
    {
        // 改进日志初始化，确保资源正确释放

        var rule = new Rule("[bold red]Hello[/]");
        rule.Justification = Justify.Left;

        AnsiConsole.Write(rule);

        AnsiConsole.Write(
        new FigletText("关键词监控")
            .LeftJustified()
            .Color(Color.Red));
        AnsiConsole.MarkupLine("[bold red]This is By @Riniba[/]");
        AnsiConsole.Markup("[bold red]开源地址：[/]");
        AnsiConsole.MarkupLine("[bold red link]https://github.com/Riniba/TelegramMonitor[/]");
        AnsiConsole.Markup("[bold red]交流群组：[/]");
        AnsiConsole.MarkupLine("[bold red link]https://t.me/RinibaGroup[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[bold red]不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！[/]");
        AnsiConsole.MarkupLine("");
    }

    /// <summary>
    /// 生成当前时间戳
    /// </summary>
    /// <returns>格式化的时间字符串</returns>
    private static string GetTimestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

    /// <summary>
    /// 写入格式化日志
    /// </summary>
    /// <param name="message">日志消息</param>
    /// <param name="color">日志颜色</param>
    private static void WriteLog(string message, string color)
    {
        var timestamp = GetTimestamp();
        AnsiConsole.MarkupLine($"[{color}][[{timestamp}]]  {message.Replace("[", "[[").Replace("]", "]]")}[/]");
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    /// <param name="message">提示消息</param>
    public static void Prompts(string message)
    {
        var rule = new Rule($"[bold blue]{message.Replace("[", "[[").Replace("]", "]]")}[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);
    }

    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="message">调试信息</param>
    public static void Debug(string message)
    {
        WriteLog(message, "grey");
    }

    /// <summary>
    /// 输出普通信息日志
    /// </summary>
    /// <param name="message">日志信息</param>
    public static void Info(string message)
    {
        WriteLog(message, "blue");
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    public static void Warning(string message)
    {
        WriteLog(message, "yellow");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    public static void Error(string message)
    {
        WriteLog(message, "red");
    }
}