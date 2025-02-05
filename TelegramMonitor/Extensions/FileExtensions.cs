namespace TelegramMonitor;

/// <summary>
/// 提供文件操作相关的扩展方法。
/// </summary>
public class FileExtensions
{
    /// <summary>
    /// 确保配置文件存在，如果不存在则创建默认的 YAML 配置文件。
    /// </summary>
    /// <param name="filePath">配置文件的完整路径。</param>
    private static void EnsureConfigFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            LogExtensions.Warning("配置文件不存在，正在创建默认的 YAML 配置文件...");
            CreateDefaultKeywordYamlFile(filePath);
            LogExtensions.Info($"已创建默认的关键词配置文件: {filePath}");
        }
    }

    /// <summary>
    /// 从 YAML 文件加载关键词配置。
    /// </summary>
    /// <param name="filePath">YAML 配置文件路径。</param>
    /// <returns>关键词配置列表。</returns>
    public static void LoadKeywordConfigs(string filePath)
    {
        EnsureConfigFileExists(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        try
        {
            var yamlContent = File.ReadAllText(filePath);
            var yamlObject = deserializer.Deserialize<List<KeywordConfig>>(yamlContent);

            if (yamlObject == null || yamlObject.Count == 0)
            {
                LogExtensions.Warning("配置文件为空,将使用默认配置");
                Constants.Keywords = CreateDefaultKeywordConfigs();
                return;
            }

            // 配置验证和处理
            for (int i = 0; i < yamlObject.Count; i++)
            {
                var config = yamlObject[i];
                // KeywordContent为空的配置项将被跳过
                if (string.IsNullOrWhiteSpace(config.KeywordContent))
                {
                    LogExtensions.Warning($"第 {i + 1} 个关键词的 KeywordContent 为空,已跳过");
                    continue;
                }

                // KeywordType和KeywordAction即使未指定也会使用默认值
                if (!Enum.IsDefined(typeof(KeywordType), config.KeywordType))
                {
                    LogExtensions.Warning($"第 {i + 1} 个关键词 '{config.KeywordContent}' 的 KeywordType 无效,使用默认值: Contains");
                    config.KeywordType = KeywordType.Contains;
                }

                if (!Enum.IsDefined(typeof(KeywordAction), config.KeywordAction))
                {
                    LogExtensions.Warning($"第 {i + 1} 个关键词 '{config.KeywordContent}' 的 KeywordAction 无效,使用默认值: Monitor");
                    config.KeywordAction = KeywordAction.Monitor;
                }

                LogExtensions.Debug("----------");
                LogExtensions.Debug($"成功加载关键词: {config.KeywordContent}");
                LogExtensions.Debug($"关键词类型: :{config.KeywordType.GetDescription()}");
                LogExtensions.Debug($"匹配方式:{config.KeywordAction.GetDescription()}");
                LogExtensions.Debug($"区分大小写:{(config.IsCaseSensitive ? "是" : "否")} ");
                LogExtensions.Debug("----------");
                LogExtensions.Debug("");
            }

            Constants.Keywords = yamlObject.Where(x => !string.IsNullOrWhiteSpace(x.KeywordContent)).ToList();
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"加载配置文件失败: {ex.Message}");
            LogExtensions.Warning("将使用默认配置");
            Constants.Keywords = CreateDefaultKeywordConfigs();
        }
    }

    /// <summary>
    /// 创建默认配置文件
    /// </summary>
    private static void CreateDefaultKeywordYamlFile(string filePath)
    {
        var defaultConfigs = CreateDefaultKeywordConfigs();
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // 首先创建说明文档
        var documentation = @"
# 关键词监控配置文件
# -----------------------------

# 配置文件说明
# KeywordContent:   关键词内容，默认为空字符串
#                  可以是普通文本、正则表达式、用户名或用户ID
#
# KeywordType:      关键词匹配类型，默认为 Contains
#                  - Contains:  包含匹配，检查消息中是否包含关键词
#                  - Regex:     正则表达式匹配，使用正则表达式进行匹配
#                  - Fuzzy:     模糊匹配，使用 ? 分隔多个关键词，所有词都匹配才算匹配
#                  - FullWord:  全字匹配，消息需要完全等于关键词
#                  - User:      用户匹配，可使用用户名或用户ID进行匹配
#
# KeywordAction:    匹配后的动作，默认为 Monitor
#                  - Monitor:   监控消息，匹配时进行记录和通知 如果是用户 则该用户的所有消息会被记录
#                  - Exclude:   排除匹配的内容，用于过滤不需要的内容 如果是用户 则该用户的所有消息不会被记录
#
# 文本样式选项:      以下选项默认都为 false
#   IsCaseSensitive:   是否区分大小写
#   IsBold:            是否使用粗体
#   IsItalic:          是否使用斜体
#   IsUnderline:       是否添加下划线
#   IsStrikeThrough:   是否添加删除线
#   IsQuote:           是否作为引用显示
#   IsMonospace:       是否使用等宽字体
#   IsSpoiler:         是否作为剧透内容处理

# ===== 示例配置 =====

- keywordContent: '你好世界'            # 示例1: 包含匹配关键词
  keywordType: Contains                #包含匹配
  keywordAction: Monitor               #监控消息
  isCaseSensitive: false               #不区分大小写
  isBold: false                        #不使用粗体
  isItalic: false                      #不使用斜体
  isUnderline: false                   #不添加下划线
  isStrikeThrough: false               #不添加删除线
  isQuote: false                       #不作为引用显示
  isMonospace: false                   #不使用等宽字体
  isSpoiler: false                     #不作为剧透内容处理

- keywordContent: '\b1[3-9]\d{9}\b'    # 示例2: 正则表达式匹配手机号
  keywordType: Regex                   #正则包含匹配
  keywordAction: Monitor               #监控消息
  isCaseSensitive: true                #区分大小写
  isBold: true                         #使用粗体
  isItalic: false                      #不使用斜体
  isUnderline: false                   #不添加下划线
  isStrikeThrough: false               #不添加删除线
  isQuote: false                       #不作为引用显示
  isMonospace: true                    #使用等宽字体
  isSpoiler: false                     #不作为剧透内容处理

- keywordContent: '早上?好?问候'        # 示例3: 模糊匹配关键词
  keywordType: Fuzzy                   #模糊匹配
  keywordAction: Monitor               #监控消息
  isCaseSensitive: false               #不区分大小写
  isBold: false                        #不使用粗体
  isItalic: true                       #使用斜体
  isUnderline: true                    #添加下划线
  isStrikeThrough: false               #不添加删除线
  isQuote: true                        #作为引用显示
  isMonospace: false                   #不使用等宽字体
  isSpoiler: false                     #不作为剧透内容处理

- keywordContent: 'riniba'             # 示例4: 用户监控
  keywordType: User                    #用户匹配
  keywordAction: Monitor               #监控消息
  isCaseSensitive: false               #不区分大小写
  isBold: false                        #不使用粗体
  isItalic: true                       #使用斜体
  isUnderline: false                   #不添加下划线
  isStrikeThrough: false               #不添加删除线
  isQuote: false                       #不作为引用显示
  isMonospace: false                   #不使用等宽字体
  isSpoiler: false                     #不作为剧透内容处理

- keywordContent: '广告'               # 示例5: 内容排除
  keywordType: Contains                #包含匹配
  keywordAction: Exclude               #排除匹配
  isCaseSensitive: false               #不区分大小写
  isBold: false                        #不使用粗体
  isItalic: false                      #不使用斜体
  isUnderline: false                   #不添加下划线
  isStrikeThrough: true                #添加删除线
  isQuote: false                       #不作为引用显示
  isMonospace: false                   #不使用等宽字体
  isSpoiler: false                     #不作为剧透内容处理
";

        // 写入文件
        File.WriteAllText(filePath, documentation);
    }

    /// <summary>
    /// 创建默认关键词配置示例
    /// </summary>
    private static List<KeywordConfig> CreateDefaultKeywordConfigs()
    {
        return new List<KeywordConfig>
        {
            new()
            {
                // 示例1：简单的文本包含匹配
                KeywordContent = "你好世界",
                KeywordType = KeywordType.Contains, // 默认包含匹配
                KeywordAction = KeywordAction.Monitor, // 默认监控动作
                // 默认所有样式选项为false
                IsBold = false,
                IsItalic = false,
                IsUnderline = false,
                IsStrikeThrough = false,
                IsQuote = false,
                IsMonospace = false,
                IsSpoiler = false,
                IsCaseSensitive = false
            },
            new()
            {
                // 示例2：使用正则表达式匹配手机号
                KeywordContent = @"\b1[3-9]\d{9}\b",
                KeywordType = KeywordType.Regex,
                KeywordAction = KeywordAction.Monitor,
                IsMonospace = true, // 使用等宽字体显示
                IsBold = true // 使用粗体显示
            },
            new()
            {
                // 示例3：模糊匹配多个关键词
                KeywordContent = "早上?好?问候", // 同时包含"早上"和"好"和"问候"
                KeywordType = KeywordType.Fuzzy,
                KeywordAction = KeywordAction.Monitor,
                IsQuote = true // 作为引用显示
            },
            new()
            {
                // 示例4：监控特定用户
                KeywordContent = "riniba", // 可以是用户名或用户ID
                KeywordType = KeywordType.User,
                KeywordAction = KeywordAction.Monitor,
                IsItalic = true // 使用斜体显示
            },
            new()
            {
                // 示例5：排除特定内容
                KeywordContent = "广告",
                KeywordType = KeywordType.Contains,
                KeywordAction = KeywordAction.Exclude, // 排除匹配的内容
                IsStrikeThrough = true // 添加删除线
            }
        };
    }

    /// <summary>
    /// 获取与用户匹配的所有“User”类型关键词配置。
    /// </summary>
    /// <param name="user">用户对象。</param>
    /// <returns>匹配到的关键词配置列表。</returns>
    public static List<KeywordConfig> GetMatchingUserConfigs(User user)
    {
        if (user == null || Constants.Keywords == null || !Constants.Keywords.Any())
        {
            return new List<KeywordConfig>();
        }

        return Constants.Keywords
            .Where(cfg => cfg.KeywordType == KeywordType.User)
            .Where(cfg => IsUserMatchingKeyword(user, cfg.KeywordContent))
            .ToList();
    }

    /// <summary>
    /// 检查用户是否匹配指定的关键字内容。
    /// </summary>
    /// <param name="user">用户对象。</param>
    /// <param name="keyword">关键字内容。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsUserMatchingKeyword(User user, string? keyword)
    {
        if (user == null || keyword == null)
        {
            return false;
        }

        return user.id.ToString() == keyword ||
               (user.ActiveUsernames?.Any(
                   username => username?.Equals(keyword, StringComparison.OrdinalIgnoreCase) == true
               ) ?? false);
    }

    /// <summary>
    /// 获取与指定消息内容匹配的所有关键词配置。
    /// </summary>
    /// <param name="message">需要检查的消息内容。</param>
    /// <returns>匹配的关键词配置列表。如果没有匹配项或输入无效，则返回空列表。</returns>
    public static List<KeywordConfig> GetMatchingKeywords(string message)
    {
        if (string.IsNullOrEmpty(message) || Constants.Keywords == null || !Constants.Keywords.Any())
        {
            return new List<KeywordConfig>();
        }

        var matchedConfigs = new List<KeywordConfig>();

        var nonUserKeywords = Constants.Keywords
            .Where(cfg => cfg.KeywordType != KeywordType.User);

        foreach (var config in nonUserKeywords)
        {
            if (IsKeywordMatching(config, message))
            {
                matchedConfigs.Add(config);
            }
        }
        return matchedConfigs;
    }

    /// <summary>
    /// 检查关键词是否与消息匹配，支持大小写选项。
    /// </summary>
    /// <param name="config">关键词配置。</param>
    /// <param name="message">原始消息。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsKeywordMatching(KeywordConfig config, string message)
    {
        if (string.IsNullOrWhiteSpace(config?.KeywordContent) || string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return config.KeywordType switch
        {
            KeywordType.Contains => IsContainsMatch(config.KeywordContent, message, config.IsCaseSensitive),
            KeywordType.Regex => IsRegexMatch(config.KeywordContent, message, config.IsCaseSensitive),
            KeywordType.Fuzzy => IsFuzzyMatch(config.KeywordContent, message, config.IsCaseSensitive),
            KeywordType.FullWord => IsFullWordMatch(config.KeywordContent, message, config.IsCaseSensitive),
            _ => false
        };
    }

    /// <summary>
    /// 检查是否包含匹配(Contains)。
    /// </summary>
    /// <param name="keyword">关键词。</param>
    /// <param name="message">原始消息内容。</param>
    /// <param name="isCaseSensitive">是否区分大小写。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsContainsMatch(string? keyword, string message, bool isCaseSensitive)
    {
        if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(message))
        {
            return false;
        }

        if (isCaseSensitive)
        {
            return message.Contains(keyword);
        }
        else
        {
            return message.ToLowerInvariant().Contains(keyword.ToLowerInvariant());
        }
    }

    /// <summary>
    /// 检查是否正则匹配(Regex)。
    /// </summary>
    /// <param name="pattern">正则表达式。</param>
    /// <param name="message">原始消息内容。</param>
    /// <param name="isCaseSensitive">是否区分大小写。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsRegexMatch(string? pattern, string message, bool isCaseSensitive)
    {
        if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        try
        {
            var options = isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var a = new Regex(pattern, options).IsMatch(message);
            return new Regex(pattern, options).IsMatch(message);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// 检查是否模糊匹配(Fuzzy)，使用 '?' 分隔多个子关键词，全部满足才算匹配。
    /// </summary>
    /// <param name="keyword">关键词(使用 ? 分隔)。</param>
    /// <param name="message">原始消息内容。</param>
    /// <param name="isCaseSensitive">是否区分大小写。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsFuzzyMatch(string? keyword, string message, bool isCaseSensitive)
    {
        if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(message))
        {
            return false;
        }

        var parts = keyword.Split('?', StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Trim())
                           .Where(p => !string.IsNullOrEmpty(p))
                           .ToArray();

        if (parts.Length == 0)
        {
            return false;
        }

        if (isCaseSensitive)
        {
            return parts.All(part => message.Contains(part));
        }
        else
        {
            var messageLower = message.ToLowerInvariant();
            return parts.All(part =>
            {
                var partLower = part.ToLowerInvariant();
                return messageLower.Contains(partLower);
            });
        }
    }

    /// <summary>
    /// 检查是否正全字匹配(FullWordMatch)。
    /// </summary>
    /// <param name="keyword">关键词。</param>
    /// <param name="message">原始消息内容。</param>
    /// <param name="isCaseSensitive">是否区分大小写。</param>
    /// <returns>true 表示匹配，false 表示不匹配。</returns>
    private static bool IsFullWordMatch(string? keyword, string message, bool isCaseSensitive)
    {
        if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(message))
        {
            return false;
        }

        if (isCaseSensitive)
        {
            return message == keyword;
        }
        else
        {
            return message.ToLowerInvariant() == keyword.ToLowerInvariant();
        }
    }
}