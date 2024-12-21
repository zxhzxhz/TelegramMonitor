using TL;
using WTelegram;

namespace TelegramMonitor;

// Telegram客户端交互类
public class TelegramServer
{
    private readonly Client _client;
    private long _sendChatId;
    private UpdateManager? _manager;
    private User? _myUser;
    private List<string> _data = new();
    private List<string> _keywords = new();

    public TelegramServer(Client client) => _client = client;

    private ChatBase? ChatBase(long id) => _manager?.Chats.GetValueOrDefault(id);

    private User? User(long id) => _manager?.Users.GetValueOrDefault(id);

    private IPeerInfo? Peer(Peer peer) => _manager?.UserOrChat(peer);

    // 处理Telegram的Update事件
    private async Task Client_OnUpdate(Update update)
    {
        try
        {
            await ProcessUpdateAsync(update);
        }
        catch (Exception ex)
        {
            Utils.Log($"处理Update时发生异常: {ex.Message}");
        }
    }

    private async Task ProcessUpdateAsync(Update update)
    {
        if (update is UpdateNewMessage unm)
            await HandleMessageAsync(unm.message);
        else if (update is UpdateEditMessage uem)
            await HandleMessageAsync(uem.message, true);
        else if (update is UpdateDeleteChannelMessages udcm)
            Utils.Log($"{udcm.messages.Length} messages deleted in {ChatBase(udcm.channel_id)}");
        else if (update is UpdateDeleteMessages udm)
            Utils.Log($"{udm.messages.Length} messages deleted");
        else if (update is UpdateUserTyping uut)
            Utils.Log($"{User(uut.user_id)} is {uut.action}");
        else
            Utils.Log(update.GetType().Name);
    }

    // 处理接收到的消息
    private async Task HandleMessageAsync(MessageBase messageBase, bool edit = false)
    {
        try
        {
            switch (messageBase)
            {
                case Message m:
                    await HandleTLMessageAsync(m);
                    break;

                case MessageService ms:
                    Utils.Log($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]");
                    break;
            }
        }
        catch (Exception ex)
        {
            Utils.Log($"处理消息时发生异常: {ex.Message}");
        }
    }

    // 处理普通消息
    private async Task HandleTLMessageAsync(Message m)
    {
        // 尝试获取有效的用户和聊天对象
        if (!TryGetValidGroupChatAndUser(m, out var groupChat, out var user))
            return;
        await HandleKeywordMatchesAsync(groupChat!, user!, m);
    }

    // 执行登录流程
    public async Task DoLoginAsync(string loginInfo)
    {
        // 执行多步登录过程
        while (_client.User == null)
        {
            var what = await _client.Login(loginInfo);
            switch (what)
            {
                case "verification_code":
                    Console.Write("验证码: ");
                    loginInfo = Console.ReadLine() ?? string.Empty;
                    break;

                case "name":
                    loginInfo = "John Doe";
                    break;

                case "password":
                    Console.Write("二级密码: ");
                    loginInfo = Console.ReadLine() ?? string.Empty;
                    break;

                default:
                    loginInfo = string.Empty;
                    break;
            }
        }

        _myUser = _client.User;
        Utils.Log($"监控人员: {_myUser} (id {_myUser.id}) 启动成功!");

        // 获取所有对话信息填充 Manager 的字典
        var dialogs = await _client.Messages_GetAllDialogs();

        // 载入关键词列表
        _keywords = Utils.LoadKeywords(Constants.KEYWORDS_FILE_PATH);

        // 从 API 获取外部数据（如广告内容）
        _data = await PeriodicHttpRequest.FetchAndProcessDataAsync();

        // 获取可管理的频道列表
        var managedChannels = GetManagedChannels(dialogs);
        var selectedChannel = SelectChannel(managedChannels);

        Utils.Log($"您已选择频道：{selectedChannel.Title} (ID: {selectedChannel.ID})");

        // 设置要发送消息的频道ID
        _sendChatId = selectedChannel.id;

        Utils.Log("开始工作!...");
        _manager = _client.WithUpdateManager(Client_OnUpdate);
        dialogs.CollectUsersChats(_manager.Users, _manager.Chats);

        // 在选定频道中发出提示信息
        await _client.SendMessageAsync(_manager.Chats[selectedChannel.ID], "开始监控!!!");

        Console.ReadKey();
    }

    // 获取可管理的频道列表
    private List<Channel> GetManagedChannels(Messages_Dialogs dialogs)
    {
        var managedChannels = new List<Channel>();
        foreach (var (id, chat) in dialogs.chats)
        {
            if (chat.IsActive && chat.IsChannel && chat is Channel channel)
            {
                if (channel.admin_rights?.flags.HasFlag(ChatAdminRights.Flags.post_messages) ?? false)
                {
                    Utils.Log($"管理频道：{channel.Title} (ID: {channel.ID})");
                    managedChannels.Add(channel);
                }
            }
        }
        return managedChannels;
    }

    // 选择一个频道作为消息发布目标
    private Channel SelectChannel(List<Channel> managedChannels)
    {
        Channel? selectedChannel = null;
        while (selectedChannel == null)
        {
            Console.Write("选择已有的管理频道ID用于发布监控信息(按enter键选择第一个): ");
            string input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                // 用户直接回车则默认选择列表中的第一个频道
                selectedChannel = managedChannels.First();
            }
            else if (long.TryParse(input, out long channelId))
            {
                selectedChannel = managedChannels.FirstOrDefault(c => c.ID == channelId);
                if (selectedChannel == null)
                {
                    Utils.Log("无效的频道ID，请重新输入。");
                }
            }
            else
            {
                Utils.Log("请输入有效的频道ID（数字）或按 Enter 键选择第一个频道。");
            }
        }

        return selectedChannel;
    }

    // 处理关键词匹配和消息转发
    private async Task HandleKeywordMatchesAsync(ChatBase chat, User user, Message message)
    {
        var matchedKeywords = Utils.GetMatchingKeywords(message.message.ToLower(), _keywords);
        if (matchedKeywords.Count == 0) return;

        var messageContent = BuildMessageContent(chat, user, message, matchedKeywords);
        await SendMonitorMessage(messageContent, message);
    }

    private string BuildMessageContent(ChatBase chat, User user, Message message, List<string> keywords)
    {
        var text = _client.EntitiesToHtml(message.message, message.entities);
        var formattedData = string.Join("\n", _data.Select(line => $"<b>{line}</b>"));
        var keywordDisplay = string.Join(", ", keywords.Select(k => $"#{k.Replace("?", "")}"));

        return $@"
<b>命中关键词：</b>{keywordDisplay}
用户ID：<code>{user.id}</code>
用户：{GetTelegramUserLink(user)}  {GetTelegramUserName(user)}
来源：<code>【{chat.Title}】</code>  {chat.MainUsername?.Insert(0, "@") ?? "无"}
时间：<code>{message.Date.AddHours(8):yyyy-MM-dd HH:mm:ss}</code>
内容：<b>{text}</b>
链接：<a href=""https://t.me/{chat.MainUsername ?? $"c/{chat.ID}"}/{message.id}"">【直达】</a>
--------------------------------
{formattedData}";
    }

    private async Task SendMonitorMessage(string content, Message originalMessage)
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
            Utils.Log($"发送消息失败: {ex.Message}");
        }
    }

    // 获取用户名
    private string GetTelegramUserName(User user) =>
        string.Join(" ", user.ActiveUsernames?.Select(u => $"@{u}") ?? Array.Empty<string>());

    // 获取用户昵称
    private string GetTelegramNickName(User user) =>
        (user.first_name + user.last_name)?.Replace("<", "").Replace(">", "") ?? string.Empty;

    // 获取用户链接
    private string GetTelegramUserLink(User user)
    {
        var nickName = GetTelegramNickName(user);
        var displayName = string.IsNullOrEmpty(nickName) ? user.id.ToString() : nickName;
        return $"<a href=\"tg://user?id={user.id}\">{displayName}</a>";
    }

    // 验证消息来源的有效性
    private bool TryGetValidGroupChatAndUser(
        Message message,
        out ChatBase? groupChat,
        out User? user)
    {
        groupChat = null;
        user = null;

        if (_manager == null || message.from_id == null) return false;

        user = User(message.from_id);
        if (user == null || user.IsBot) return false;

        var chatBase = ChatBase(message.peer_id);
        if (chatBase == null || !chatBase.IsGroup || string.IsNullOrWhiteSpace(message.message))
            return false;

        groupChat = chatBase;
        return true;
    }
}