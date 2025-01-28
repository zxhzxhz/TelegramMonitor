namespace TelegramMonitor;

/// <summary>
/// Telegram 客户端交互类，用于登录、获取对话并处理消息更新等功能。
/// </summary>
public class TelegramManager
{
    private readonly Client _client;
    private readonly PeriodicTaskManager _taskManager;
    private long _sendChatId;
    private UpdateManager? _manager;
    private User? _myUser;

    /// <summary>
    /// 初始化 <see cref="TelegramManager"/> 实例，并绑定指定的 <see cref="Client"/> 对象。
    /// </summary>
    /// <param name="client">Telegram 客户端对象，用于与 Telegram API 通信。</param>
    public TelegramManager(Client client)
    {
        _client = client;
        _taskManager = new PeriodicTaskManager();
    }

    #region 私有辅助方法

    /// <summary>
    /// 根据聊天编号获取 <see cref="ChatBase"/> 对象。
    /// </summary>
    /// <param name="id">聊天编号。</param>
    /// <returns>返回对应的 <see cref="ChatBase"/>，若不存在则返回 null。</returns>
    private ChatBase? ChatBase(long id)
    {
        return _manager?.Chats.GetValueOrDefault(id);
    }

    /// <summary>
    /// 根据用户编号获取 <see cref="User"/> 对象。
    /// </summary>
    /// <param name="id">用户编号。</param>
    /// <returns>返回对应的 <see cref="User"/>，若不存在则返回 null。</returns>
    private User? User(long id)
    {
        return _manager?.Users.GetValueOrDefault(id);
    }

    /// <summary>
    /// 将 <see cref="Peer"/> 对象转换为 <see cref="IPeerInfo"/> 接口实例。
    /// </summary>
    /// <param name="peer">需要转换的 Peer。</param>
    /// <returns>返回对应的 <see cref="IPeerInfo"/>，若无法获取则返回 null。</returns>
    private IPeerInfo? Peer(Peer peer)
    {
        return _manager?.UserOrChat(peer);
    }

    #endregion 私有辅助方法

    #region 更新处理

    /// <summary>
    /// Telegram 客户端 Update 事件回调。
    /// </summary>
    /// <param name="update">接收的更新对象。</param>
    /// <returns>表示异步执行结果的 <see cref="Task"/>。</returns>
    private async Task Client_OnUpdate(Update update)
    {
        try
        {
            await ProcessUpdateAsync(update);
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"处理Update时发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理特定的 Telegram 更新。
    /// </summary>
    /// <param name="update">Telegram 更新对象。</param>
    /// <returns>表示异步执行结果的 <see cref="Task"/>。</returns>
    private async Task ProcessUpdateAsync(Update update)
    {
        switch (update)
        {
            case UpdateNewMessage unm:
                await HandleMessageAsync(unm.message);
                break;

            case UpdateEditMessage uem:
                LogExtensions.Debug($"{User(uem.message.From)} edited a message in {ChatBase(uem.message.Peer)}");
                break;

            case UpdateDeleteChannelMessages udcm:
                LogExtensions.Debug($"{udcm.messages.Length} messages deleted in {ChatBase(udcm.channel_id)}");
                break;

            case UpdateDeleteMessages udm:
                LogExtensions.Debug($"{udm.messages.Length} messages deleted ");
                break;

            case UpdateUserTyping uut:
                LogExtensions.Debug($"{User(uut.user_id)} is {uut.action}");
                break;

            case UpdateChatUserTyping ucut:
                LogExtensions.Debug($"{Peer(ucut.from_id)} is {ucut.action} in {ChatBase(ucut.chat_id)}");
                break;

            case UpdateChannelUserTyping ucut2:
                LogExtensions.Debug($"{Peer(ucut2.from_id)} is {ucut2.action} in {ChatBase(ucut2.channel_id)}");
                break;

            case UpdateChatParticipants { participants: ChatParticipants cp }:
                LogExtensions.Debug($"{cp.participants.Length} participants in {ChatBase(cp.chat_id)}");
                break;

            case UpdateUserStatus uus:
                LogExtensions.Debug($"{User(uus.user_id)} is now {uus.status.GetType().Name[10..]}");
                break;

            case UpdateUserName uun:
                LogExtensions.Debug($"{User(uun.user_id)} changed profile name: {uun.first_name} {uun.last_name}");
                break;

            case UpdateUser uu:
                LogExtensions.Debug($"{User(uu.user_id)} changed infos/photo");
                break;

            default:
                LogExtensions.Debug(update.GetType().Name);
                break;
        }
    }

    /// <summary>
    /// 处理接收到的消息（可能是普通消息或服务消息）。
    /// </summary>
    /// <param name="messageBase">消息基类对象。</param>
    /// <param name="edit">是否为编辑后的消息。</param>
    /// <returns>表示异步执行结果的 <see cref="Task"/>。</returns>
    private async Task HandleMessageAsync(MessageBase messageBase, bool edit = false)
    {
        if (edit) return;

        try
        {
            switch (messageBase)
            {
                case Message m:
                    await HandleTLMessageAsync(m);
                    break;

                case MessageService ms:
                    LogExtensions.Debug($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]");
                    break;
            }
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"处理消息时发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理纯文本消息。
    /// </summary>
    /// <param name="m">消息对象。</param>
    /// <returns>表示异步执行结果的 <see cref="Task"/>。</returns>
    private async Task HandleTLMessageAsync(Message m)
    {
        if (!TryGetValidGroupChatAndUser(m, out var groupChat, out var user))
            return;

        await HandleKeywordMatchesAsync(groupChat!, user!, m);
    }

    #endregion 更新处理

    #region 公共方法

    /// <summary>
    /// 执行登录流程，等待用户输入验证码、二级密码等信息。
    /// </summary>
    /// <param name="loginInfo">初始登录信息或验证码。</param>
    /// <returns>表示异步登录过程的 <see cref="Task"/>。</returns>
    public async Task DoLoginAsync(string loginInfo)
    {
        while (_client.User == null)
        {
            var what = await _client.Login(loginInfo);
            switch (what)
            {
                case "verification_code":
                    LogExtensions.Prompts("验证码: ");
                    loginInfo = Console.ReadLine() ?? string.Empty;
                    break;

                case "name":
                    loginInfo = "by riniba";
                    break;

                case "password":
                    LogExtensions.Prompts("二级密码: ");
                    loginInfo = Console.ReadLine() ?? string.Empty;
                    break;

                default:
                    loginInfo = string.Empty;
                    break;
            }
        }

        _myUser = _client.User;
        LogExtensions.Info($"监控人员: {_myUser} (id {_myUser.id}) 启动成功!");

        var dialogs = await _client.Messages_GetAllDialogs();

        FileExtensions.LoadKeywordConfigs(Constants.FilePaths.KeywordsFile);
        await HttpExtensions.FetchAndProcessDataAsync();

        await GetManagedChatAsync(dialogs);
    }

    #endregion 公共方法

    #region 私有业务逻辑

    /// <summary>
    /// 启动接收并处理 Telegram 更新的核心逻辑。
    /// </summary>
    /// <param name="dialogs">包含用户和聊天信息的对话列表。</param>
    private void StartTelegram(Messages_Dialogs dialogs)
    {
        LogExtensions.Info("开始工作!...");
        _manager = _client.WithUpdateManager(Client_OnUpdate);
        dialogs.CollectUsersChats(_manager.Users, _manager.Chats);

        _taskManager.Start();
        Console.ReadKey();
    }

    /// <summary>
    /// 让用户选择一个可发送消息的聊天频道或群组，并进行后续监控处理。
    /// </summary>
    /// <param name="dialogs">包含所有聊天信息的对话列表。</param>
    /// <returns>表示异步执行结果的 <see cref="Task"/>。</returns>
    private async Task GetManagedChatAsync(Messages_Dialogs dialogs)
    {
        var availableChats = new List<ChatBase>();
        ChatBase? selectedChat = null;

        var prompt = new SelectionPrompt<ChatBase>()
            .Title("选择监控消息发布的目标")
            .PageSize(10)
            .UseConverter(chat => Markup.Escape(chat.Title));

        foreach (var (id, chat) in dialogs.chats)
        {
            if (!chat.IsActive) continue;

            bool canSendMessages = chat switch
            {
                Chat smallgroup => !smallgroup.IsBanned(ChatBannedRights.Flags.send_messages),
                Channel channel when channel.IsChannel => !channel.IsBanned(ChatBannedRights.Flags.send_messages),
                Channel group => !group.IsBanned(ChatBannedRights.Flags.send_messages),
                _ => false
            };

            if (canSendMessages)
            {
                availableChats.Add(chat);
                prompt.AddChoice(chat);
            }
        }

        if (availableChats.Count == 0)
        {
            LogExtensions.Error("未找到任何可发送消息的频道或群组！");
            return;
        }

        while (selectedChat == null)
        {
            LogExtensions.Prompts("选择要发送监控消息的目标:");
            selectedChat = AnsiConsole.Prompt(prompt);

            if (selectedChat == null)
            {
                LogExtensions.Error("无效的选择，请重新选择一个有效的目标。");
                continue;
            }

            LogExtensions.Info($"您已选择：{selectedChat.Title} (ID: {selectedChat.ID})");
            try
            {
                await _client.SendMessageAsync(selectedChat, "软件就绪!开始监控！");
                _sendChatId = selectedChat.ID;
                StartTelegram(dialogs);
            }
            catch (Exception e)
            {
                LogExtensions.Error($"{e.Message}");
                selectedChat = null;
            }
        }
    }

    /// <summary>
    /// 根据用户和消息内容匹配关键词配置，并根据匹配结果执行对应的处理逻辑（排除或监控）。
    /// </summary>
    /// <param name="chat">当前聊天对象。</param>
    /// <param name="user">消息发送者。</param>
    /// <param name="message">消息对象。</param>
    /// <returns>异步执行结果的 <see cref="Task"/>。</returns>
    private async Task HandleKeywordMatchesAsync(ChatBase chat, User user, Message message)
    {
        if (string.IsNullOrWhiteSpace(message.message))
            return;

        var matchedUserKeywords = FileExtensions.GetMatchingUserConfigs(user);

        if (matchedUserKeywords.Any(x => x.KeywordAction == KeywordAction.Exclude))
        {
            LogExtensions.Debug($"{TelegramExtensions.GetTelegramNickName(user)}（ID:{user.id}） 此用户在排除列表内,跳过");
            return;
        }

        if (matchedUserKeywords.Any(x => x.KeywordAction == KeywordAction.Monitor))
        {
            LogExtensions.Debug($"{TelegramExtensions.GetTelegramNickName(user)}（ID:{user.id}） 此用户在无论发送什么都会监控");
            var userMonitorContent = BuildMessageContent(chat, user, message, matchedUserKeywords);
            await SendMonitorMessageAsync(userMonitorContent, message);
            return;
        }

        LogExtensions.Debug($"{TelegramExtensions.GetTelegramNickName(user)}（ID:{user.id}） 在 {chat.Title} 中发送：{message.message}");

        var matchedKeywords = FileExtensions.GetMatchingKeywords(message.message);

        if (matchedKeywords.Any(x => x.KeywordAction == KeywordAction.Exclude))
        {
            LogExtensions.Debug("消息包含排除的关键词，跳过处理");
            return;
        }

        matchedKeywords = matchedKeywords
            .Where(x => x.KeywordAction == KeywordAction.Monitor)
            .ToList();

        if (matchedKeywords.Count == 0)
        {
            LogExtensions.Debug("无匹配关键词，跳过");
            return;
        }

        var messageContent = BuildMessageContent(chat, user, message, matchedKeywords);
        await SendMonitorMessageAsync(messageContent, message);
    }

    /// <summary>
    /// 构建要发送到监控频道的消息内容字符串，并将整条消息应用合并样式。
    /// </summary>
    /// <param name="chat">聊天对象。</param>
    /// <param name="user">消息发送者。</param>
    /// <param name="message">原始消息。</param>
    /// <param name="keywords">匹配到的关键词配置列表。</param>
    /// <returns>拼接生成的 HTML 字符串。</returns>
    private string BuildMessageContent(ChatBase chat, User user, Message message, List<KeywordConfig> keywords)
    {
        var text = _client.EntitiesToHtml(message.message, message.entities);

        var mergedStyle = TelegramExtensions.MergeKeywordStyles(keywords);

        text = TelegramExtensions.ApplyStylesToText(text, mergedStyle);

        var formattedData = string.Join("\n", Constants.SystemConfig.Advertisement.Select(line => $"<b>{line}</b>"));

        var keywordPlainList = string.Join(", ", keywords.Select(k => k.KeywordContent));
        LogExtensions.Warning($"匹配到关键词: {keywordPlainList}");

        return $@"
<b>命中关键词：</b>#{keywordPlainList}
用户ID：<code>{user.id}</code>
用户：{TelegramExtensions.GetTelegramUserLink(user)}  {TelegramExtensions.GetTelegramUserName(user)}
来源：<code>【{chat.Title}】</code>  {chat.MainUsername?.Insert(0, "@") ?? "无"}
时间：<code>{message.Date.AddHours(8):yyyy-MM-dd HH:mm:ss}</code>
内容：{text}
链接：<a href=""https://t.me/{chat.MainUsername ?? $"c/{chat.ID}"}/{message.id}"">【直达】</a>
--------------------------------
{formattedData}";
    }

    /// <summary>
    /// 向监控频道或群组发送匹配到的消息内容。
    /// </summary>
    /// <param name="content">待发送的文本内容（HTML 格式）。</param>
    /// <param name="originalMessage">原始消息对象，用于携带媒体信息。</param>
    /// <returns>表示异步发送操作的 <see cref="Task"/>。</returns>
    private async Task SendMonitorMessageAsync(string content, Message originalMessage)
    {
        try
        {
            var chat = ChatBase(_sendChatId);
            if (chat == null) return;

            var entities = _client.HtmlToEntities(ref content, users: _manager?.Users);
            await _client.SendMessageAsync(chat, content,
                preview: Client.LinkPreview.Disabled,
                entities: entities,
                media: originalMessage.media?.ToInputMedia());
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"发送消息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查是否能在当前消息中识别到有效的群组聊天与用户对象。
    /// </summary>
    /// <param name="message">消息对象。</param>
    /// <param name="groupChat">输出参数，返回匹配到的群组对象。</param>
    /// <param name="user">输出参数，返回匹配到的用户对象。</param>
    /// <returns>若识别成功返回 true，否则返回 false。</returns>
    private bool TryGetValidGroupChatAndUser(Message message, out ChatBase? groupChat, out User? user)
    {
        groupChat = null;
        user = null;

        if (_manager == null || message.from_id == null)
            return false;

        user = User(message.from_id);
        if (user == null || user.IsBot)
            return false;

        var chatBase = ChatBase(message.peer_id);
        if (chatBase == null || !chatBase.IsGroup || string.IsNullOrWhiteSpace(message.message))
            return false;

        groupChat = chatBase;
        return true;
    }

    #endregion 私有业务逻辑
}