using System.Text.RegularExpressions;

namespace TelegramMonitor;

public static partial class Utils
{
    // 使用编译过的正则表达式进行电话号码校验，提高匹配效率
    // 正则要求:
    // 1. 以 "+" 开头
    // 2. 后跟 10~15 位数字（满足国际E.164标准）
    private static readonly Regex PhoneNumberRegex = new Regex(@"^\+\d{10,15}$", RegexOptions.Compiled);

    /// <summary>
    /// 校验电话号码是否符合 E.164 格式要求（以 "+" 开头，后跟10至15位数字）。
    /// </summary>
    /// <param name="phoneNumber">需要验证的电话号码字符串</param>
    /// <returns>若格式合法返回 true，否则返回 false</returns>
    public static bool IsPhoneNumberValid(string phoneNumber) => PhoneNumberRegex.IsMatch(phoneNumber);

    /// <summary>
    /// 打印日志信息到控制台，包含时间戳以帮助在生产环境中进行问题跟踪。
    /// </summary>
    /// <param name="log">需要输出的日志内容</param>
    public static void Log(string log)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{log}");
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
    /// 从指定的关键词文件中加载关键词列表。
    /// 如果文件不存在则自动创建一个包含默认关键词内容的文件。
    /// 方法将过滤掉空行和仅包含空白字符的行，并返回剩余行的列表。
    /// </summary>
    /// <param name="filePath">关键词文件的路径。</param>
    /// <returns>包含非空行的字符串列表，如果读取失败或为空则返回空列表。</returns>
    public static List<string> LoadKeywords(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Utils.Log("关键词文件路径无效，返回空列表。");
            return new List<string>();
        }

        try
        {
            // 如果文件不存在则创建默认文件并写入默认关键词内容
            if (!System.IO.File.Exists(filePath))
            {
                Utils.Log("关键词文件未找到，正在创建新文件...");

                // 默认关键词内容
                string defaultContent = $"关键词说明:{Environment.NewLine}" +
                    $"第一行是全字匹配，只匹配含有关键词1的消息{Environment.NewLine}" +
                    $"第二行是模糊匹配，匹配含有关键词2和关键词3同时存在的消息{Environment.NewLine}" +
                    $"第三行是模糊匹配，匹配含有关键词4和关键词5同时存在的消息{Environment.NewLine}" +
                    $"所有匹配的关键词不区分大小写{Environment.NewLine}" +
                    $"关键词1{Environment.NewLine}" +
                    $"关键词2?关键词3{Environment.NewLine}" +
                    $"关键词4?关键词5";
                System.IO.File.WriteAllText(filePath, defaultContent);
            }

            // 读取文件的所有行，过滤掉空行或空白行 并转换为小写
            var lines = System.IO.File.ReadAllLines(filePath)
                                      .Where(line => !string.IsNullOrWhiteSpace(line))
                                      .Select(k => k.ToLower())
                                      .ToList();

            return lines;
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，返回空列表以避免外部逻辑中断
            Utils.Log($"读取关键词文件时发生错误: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// 从给定消息文本中查找与关键词列表匹配的关键词集合。
    /// 匹配规则：
    /// 每个关键词可能包含多个用 "?" 分隔的子关键词，只有当消息文本中全部子关键词都出现时才算匹配。
    /// </summary>
    /// <param name="messageLower">已转换为小写的消息内容</param>
    /// <param name="keywords">关键词列表</param>
    /// <returns>所有匹配成功的关键词列表</returns>
    public static List<string> GetMatchingKeywords(string messageLower, List<string> keywords)
    {
        var matched = new List<string>();

        foreach (var keyword in keywords)
        {
            // 拆分子关键词并去除多余空白字符
            var keywordParts = keyword
                .Split('?')
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            // 判断该关键词中的所有子关键词是否都出现在消息中
            bool isMatch = keywordParts.All(kw => messageLower.Contains(kw));
            if (isMatch)
            {
                matched.Add(keyword);
            }
        }

        return matched;
    }
}