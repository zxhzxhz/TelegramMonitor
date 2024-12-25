using System.Text.RegularExpressions;

namespace TelegramMonitor;

/// <summary>
/// 提供通用工具方法的静态类，包含电话号码验证、日志记录、关键词处理等功能。
/// </summary>
public static class Utils
{
    //验证电话号码格式的正则表达式
    private static readonly Regex PhoneNumberRegex = new(@"^\+\d{10,15}$", RegexOptions.Compiled);

    //验证电话号码是否符合E.164国际标准格式
    public static bool IsPhoneNumberValid(string? phoneNumber) =>
        !string.IsNullOrEmpty(phoneNumber) && PhoneNumberRegex.IsMatch(phoneNumber);

    //记录日志信息
    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    //循环请求用户输入电话号码，直到格式合法
    public static string PromptForPhoneNumber()
    {
        string phoneNumber;
        do
        {
            Console.Write("请输入telegram绑定的电话号码: ");

            // 去除用户输入中空白字符，避免格式不统一（如在输入中不小心敲入空格）
            phoneNumber = (Console.ReadLine() ?? string.Empty).Replace(" ", "");

            if (!IsPhoneNumberValid(phoneNumber))
            {
                Log("电话号码格式无效，请重新输入。正确格式示例：+8613812345678");
            }
        } while (!IsPhoneNumberValid(phoneNumber));

        return phoneNumber;
    }

    // 从指定文件路径加载关键词列表
    public static List<string> LoadKeywords(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log("无效的关键词文件路径");
            return new List<string>();
        }

        try
        {
            if (!File.Exists(filePath))
            {
                CreateDefaultKeywordsFile(filePath);
            }

            return File.ReadAllLines(filePath)
                      .Where(line => !string.IsNullOrWhiteSpace(line))
                      .Select(line => line.ToLowerInvariant())
                      .ToList();
        }
        catch (Exception ex)
        {
            Log($"读取关键词文件失败: {ex.Message}");
            return new List<string>();
        }
    }

    // 获取与消息匹配的关键词列表
    public static List<string> GetMatchingKeywords(string message, List<string> keywords)
    {
        if (string.IsNullOrEmpty(message) || keywords == null || !keywords.Any())
        {
            return new List<string>();
        }

        var messageLower = message.ToLowerInvariant();
        return keywords.Where(keyword =>
        {
            var parts = keyword.Split('?', StringSplitOptions.RemoveEmptyEntries)
                              .Select(k => k.Trim())
                              .Where(k => !string.IsNullOrEmpty(k))
                              .ToArray();

            return parts.Length > 0 && parts.All(part =>
                messageLower.Contains(part, StringComparison.OrdinalIgnoreCase));
        }).ToList();
    }

    // 创建默认的关键词文件
    private static void CreateDefaultKeywordsFile(string filePath)
    {
        Log("创建默认关键词文件...");
        const string defaultContent =
            "# 关键词配置说明\n" +
            "# 1. 每行一个关键词规则\n" +
            "# 2. 使用?分隔多个关键词，表示AND关系\n" +
            "# 3. 不区分大小写\n" +
            "# 4. 支持模糊匹配\n\n" +
            "关键词1\n" +
            "关键词2?关键词3\n" +
            "关键词4?关键词5";

        File.WriteAllText(filePath, defaultContent);
    }
}