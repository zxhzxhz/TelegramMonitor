using Spectre.Console;

namespace TelegramMonitor;

public static class LogExtensions
{
    //打印Logo
    public static void Logo()
    {
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

    // 公共时间格式
    private static string GetTimestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

    // 公共的日志输出方法
    private static void WriteLog(string message, string color)
    {
        var timestamp = GetTimestamp();
        AnsiConsole.MarkupLine($"[{color}][[{timestamp}]]  {message}[/]");
    }

    // 输出普通的提示信息（可以为任何消息类型）
    public static void Prompts(string message)
    {
        var rule = new Rule($"[bold blue]{message}[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);
    }

    // Debug日志
    public static void Debug(string message)
    {
        WriteLog(message, "grey");
    }

    // 信息日志
    public static void Info(string message)
    {
        WriteLog(message, "blue");
    }

    // 警告日志
    public static void Warning(string message)
    {
        WriteLog(message, "yellow");
    }

    // 错误日志
    public static void Error(string message)
    {
        WriteLog(message, "red");
    }
}