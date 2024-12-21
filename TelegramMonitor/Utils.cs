using System.Text.RegularExpressions;

namespace TelegramMonitor;

/// <summary>
/// 提供通用工具方法的静态类，包含电话号码验证、日志记录、关键词处理等功能。
/// </summary>
public static class Utils
{
    /// <summary>
    /// 用于验证电话号码格式的编译后正则表达式
    /// 格式要求：以"+"开头，后跟10-15位数字(E.164国际标准)
    /// </summary>
    private static readonly Regex PhoneNumberRegex = new(@"^\+\d{10,15}$", RegexOptions.Compiled);

    /// <summary>
    /// 验证电话号码是否符合E.164国际标准格式
    /// </summary>
    /// <param name="phoneNumber">待验证的电话号码字符串</param>
    /// <returns>如果电话号码有效，返回true；否则返回false</returns>
    public static bool IsPhoneNumberValid(string? phoneNumber) => 
        !string.IsNullOrEmpty(phoneNumber) && PhoneNumberRegex.IsMatch(phoneNumber);

    /// <summary>
    /// 记录日志信息
    /// </summary>
    /// <param name="message">日志信息内容</param>
    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    /// <summary>
    /// 循环向用户提示并获取电话号码输入，直至电话号码满足 E.164 标准格式。
    /// </summary>
    /// <returns>返回经过格式验证的电话号码字符串（E.164 格式）</returns>
    public static string PromptForPhoneNumber()
    {
        string phoneNumber;
        do
        {
            Console.Write("请输入您的电话号码: ");

            // 去除用户输入中空白字符，避免格式不统一（如在输入中不小心敲入空格）
            phoneNumber = (Console.ReadLine() ?? string.Empty).Replace(" ", "");

            if (!IsPhoneNumberValid(phoneNumber))
            {
                Log("电话号码格式无效，请重新输入。正确格式示例：+8613812345678");
            }
        } while (!IsPhoneNumberValid(phoneNumber));

        return phoneNumber;
    }

    /// <summary>
    /// 从指定文件路径加载关键词列表
    /// </summary>
    /// <param name="filePath">关键词文件路径</param>
    /// <returns>返回关键词列表</returns>
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

    /// <summary>
    /// 获取与消息匹配的关键词列表
    /// </summary>
    /// <param name="message">待匹配的消息内容</param>
    /// <param name="keywords">关键词列表</param>
    /// <returns>返回匹配的关键词列表</returns>
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

    /// <summary>
    /// 创建默认的关键词文件
    /// </summary>
    /// <param name="filePath">关键词文件路径</param>
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