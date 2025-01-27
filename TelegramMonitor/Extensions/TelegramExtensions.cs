namespace TelegramMonitor;

/// <summary>
/// 提供 Telegram 用户信息相关的扩展方法
/// </summary>
public static class TelegramExtensions
{
    /// <summary>
    /// 获取用户的 Telegram 用户名列表
    /// </summary>
    /// <param name="user">Telegram 用户对象</param>
    /// <returns>格式化后的用户名列表，以空格分隔，每个用户名前带@符号</returns>
    public static string GetTelegramUserName(User user)
    {
        if (user?.ActiveUsernames == null || !user.ActiveUsernames.Any())
            return string.Empty;

        return string.Join(" ", user.ActiveUsernames.Select(u => $"@{u}"));
    }

    /// <summary>
    /// 获取用户的昵称（姓名）
    /// </summary>
    /// <param name="user">Telegram 用户对象</param>
    /// <returns>经过安全转义的用户昵称</returns>
    public static string GetTelegramNickName(User user)
    {
        if (user == null)
            return string.Empty;

        var fullName = $"{user.first_name}{user.last_name}".Trim();
        return string.IsNullOrEmpty(fullName)
            ? string.Empty
            : SecurityElement.Escape(fullName);
    }

    /// <summary>
    /// 生成用户的 Telegram 链接
    /// </summary>
    /// <param name="user">Telegram 用户对象</param>
    /// <returns>HTML 格式的用户链接</returns>
    public static string GetTelegramUserLink(User user)
    {
        if (user == null)
            return string.Empty;

        var displayName = GetTelegramNickName(user);
        if (string.IsNullOrEmpty(displayName))
            displayName = user.id.ToString();

        return $"<a href=\"tg://user?id={user.id}\">{displayName}</a>";
    }

    /// <summary>
    /// 合并多个关键词配置的样式选项，使用“或(OR)”逻辑产生最终的样式。
    /// </summary>
    /// <param name="keywords">匹配到的多个关键词配置。</param>
    /// <returns>合并后的样式配置。</returns>
    public static KeywordConfig MergeKeywordStyles(List<KeywordConfig> keywords)
    {
        // 如果一个都没有匹配，则返回“全部 false”的空样式
        if (keywords.Count == 0)
            return new KeywordConfig();

        // OR 逻辑合并
        // 只要任意一个关键词需要某项样式，就对整条消息启用该样式
        return new KeywordConfig
        {
            IsBold = keywords.Any(k => k.IsBold),
            IsItalic = keywords.Any(k => k.IsItalic),
            IsUnderline = keywords.Any(k => k.IsUnderline),
            IsStrikeThrough = keywords.Any(k => k.IsStrikeThrough),
            IsQuote = keywords.Any(k => k.IsQuote),
            IsMonospace = keywords.Any(k => k.IsMonospace),
            IsSpoiler = keywords.Any(k => k.IsSpoiler)
        };
    }

    /// <summary>
    /// 根据合并后的样式选项，将整段文本一次性包裹上相应的标签。
    /// </summary>
    /// <param name="text">原始文本（可带已有 HTML 标签）。</param>
    /// <param name="styleConfig">合并后的一组样式开关。</param>
    /// <returns>添加完样式标签后的文本。</returns>
    public static string ApplyStylesToText(string text, KeywordConfig styleConfig)
    {
        // 注意 Telegram 对某些标签的支持，请根据实际需要调整顺序或用法
        var result = text;

        // 1. 剧透（某些客户端使用 <tg-spoiler>）
        if (styleConfig.IsSpoiler)
        {
            result = $"<tg-spoiler>{result}</tg-spoiler>";
        }

        // 2. 等宽
        if (styleConfig.IsMonospace)
        {
            // Telegram中可以用 <code>...</code> 或 <pre>...</pre>
            result = $"<code>{result}</code>";
        }

        // 3. 引用
        if (styleConfig.IsQuote)
        {
            // <blockquote> 在 Telegram 未必渲染效果理想，仅作示例
            result = $"<blockquote>{result}</blockquote>";
        }

        // 4. 加粗
        if (styleConfig.IsBold)
        {
            result = $"<b>{result}</b>";
        }

        // 5. 斜体
        if (styleConfig.IsItalic)
        {
            result = $"<i>{result}</i>";
        }

        // 6. 底线
        if (styleConfig.IsUnderline)
        {
            result = $"<u>{result}</u>";
        }

        // 7. 删除线
        if (styleConfig.IsStrikeThrough)
        {
            result = $"<s>{result}</s>";
        }

        return result;
    }

    /// <summary>
    /// 异步运行主循环，监听用户输入以停止程序。
    /// </summary>
    /// <param name="cancellationToken">用于监测取消请求的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public static async Task RunMainLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (input?.ToLowerInvariant() == "stop")
            {
                LogExtensions.Warning("正在停止程序...");
                break;
            }
            await Task.Delay(1000, cancellationToken);
        }
    }
}