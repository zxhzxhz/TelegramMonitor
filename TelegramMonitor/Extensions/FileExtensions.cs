namespace TelegramMonitor;

public class FileExtensions
{
    // 从指定文件路径加载关键词列表
    public static void LoadKeywords(string filePath)
    {
        LogExtensions.Debug("开始读取关键词列表...");

        if (string.IsNullOrWhiteSpace(filePath))
        {
            LogExtensions.Error("无效的关键词文件路径");
            return;
        }

        try
        {
            if (!File.Exists(filePath))
            {
                CreateDefaultKeywordsFile(filePath);
            }

            var list = File.ReadAllLines(filePath)
                      .Where(line => !string.IsNullOrWhiteSpace(line))
                      .Select(line => line.ToLowerInvariant())
                      .ToList();

            Constants.KEYWORDS = list;
            LogExtensions.Debug("读取关键词文件成功");
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"读取关键词文件失败: {ex.Message}");
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
        LogExtensions.Debug("创建默认关键词文件...");
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