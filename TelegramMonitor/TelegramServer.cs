using TL;
using WTelegram;

namespace TelegramMonitor;

/// <summary>
/// TelegramUtil 类负责与 Telegram 客户端（WTelegram.Client）进行交互，包括登录、消息处理、
/// 关键词匹配和消息转发等逻辑。
/// </summary>
public class TelegramServer
{
    private readonly Client _client;             // Telegram客户端实例
    private long _sendChatId = 0;                // 要发送监控消息的频道ID
    private WTelegram.UpdateManager? _manager;   // 更新管理器，用于处理Telegram下发的各种Update事件
    private User? _myUser;                       // 登录后当前账户的用户对象
    private List<string> _data = new();          // 外部获取的数据（如广告内容），发送消息中可引用
    private List<string> _keywords = new();      // 关键词列表，用于匹配消息文本

    public TelegramServer(Client client)
    {
        _client = client;
    }

    /// <summary>
    /// 根据指定的聊天 ID 获取对应的 ChatBase 对象。
    /// 如果不存在或 _manager 未初始化，则返回 null。
    /// </summary>
    private ChatBase? Chat(long id) =>
        _manager != null && _manager.Chats.TryGetValue(id, out var chat) ? chat : null;

    /// <summary>
    /// 根据用户 ID 获取对应的 User 对象。
    /// 如果不存在或 _manager 未初始化，则返回 null。
    /// </summary>
    private User? User(long id) =>
        _manager != null && _manager.Users.TryGetValue(id, out var user) ? user : null;

    /// <summary>
    /// 从给定的 Peer 对象获取对应的用户或聊天信息（IPeerInfo）。
    /// 如果 _manager 未初始化，则返回 null。
    /// </summary>
    private IPeerInfo? Peer(Peer peer) => _manager?.UserOrChat(peer);

    /// <summary>
    /// 当接收到 Update 时的回调方法，根据 Update 类型调用不同的处理逻辑。
    /// </summary>
    private async Task Client_OnUpdate(Update update)
    {
        try
        {
            switch (update)
            {
                case UpdateNewMessage unm:
                    await HandleMessageAsync(unm.message);
                    break;

                case UpdateEditMessage uem:
                    await HandleMessageAsync(uem.message, true);
                    break;

                case UpdateDeleteChannelMessages udcm:
                    Utils.Log($"{udcm.messages.Length} message(s) deleted in {Chat(udcm.channel_id)}");
                    break;

                case UpdateDeleteMessages udm:
                    Utils.Log($"{udm.messages.Length} message(s) deleted");
                    break;

                case UpdateUserTyping uut:
                    Utils.Log($"{User(uut.user_id)} is {uut.action}");
                    break;

                case UpdateChatUserTyping ucut:
                    Utils.Log($"{Peer(ucut.from_id)} is {ucut.action} in {Chat(ucut.chat_id)}");
                    break;

                case UpdateChannelUserTyping ucut2:
                    Utils.Log($"{Peer(ucut2.from_id)} is {ucut2.action} in {Chat(ucut2.channel_id)}");
                    break;

                case UpdateChatParticipants { participants: ChatParticipants cp }:
                    Utils.Log($"{cp.participants.Length} participants in {Chat(cp.chat_id)}");
                    break;

                case UpdateUserStatus uus:
                    Utils.Log($"{User(uus.user_id)} is now {uus.status.GetType().Name[10..]}");
                    break;

                case UpdateUserName uun:
                    Utils.Log($"{User(uun.user_id)} changed profile name: {uun.first_name} {uun.last_name}");
                    break;

                case UpdateUser uu:
                    Utils.Log($"{User(uu.user_id)} changed infos/photo");
                    break;

                default:
                    Utils.Log(update.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            Utils.Log($"处理Update时发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理接收到的消息，根据消息类型调用相应方法。
    /// </summary>
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

    /// <summary>
    /// 处理常规消息（Message类型）。
    /// 判断消息是否来自群组且非bot用户后，进行关键词匹配。
    /// </summary>
    private async Task HandleTLMessageAsync(Message m)
    {
        // 尝试获取有效的用户和聊天对象
        if (!TryGetValidChannelAndUser(m, out var channel, out var user))
            return;
        await HandleKeywordMatchesAsync(channel!, user!, m);
    }

    /// <summary>
    /// 执行登录流程，包含多步验证（验证码、二级密码等），
    /// 登录成功后初始化用户、管理器、关键词列表、外部数据，并让用户选择发送消息的频道。
    /// </summary>
    /// <param name="loginInfo">登录信息，如手机号或验证码</param>
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
        _keywords = Utils.LoadKeywords(Constants.KeywordsFile);

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

    /// <summary>
    /// 从对话列表中获取当前用户可管理的频道列表。
    /// 条件：Channel已激活 (IsActive) 且有发消息权限(post_messages)。
    /// </summary>
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

    /// <summary>
    /// 引导用户从可管理的频道列表中选择一个频道作为消息发布目标。
    /// 用户可通过输入频道ID或按回车键选择第一个频道。
    /// </summary>
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

    /// <summary>
    /// 对消息进行关键词匹配，如匹配成功则发送包含相关信息的消息到选定频道。
    /// </summary>
    /// <param name="channel">消息所属频道</param>
    /// <param name="user">消息发送者</param>
    /// <param name="message">消息对象</param>
    private async Task HandleKeywordMatchesAsync(Channel channel, User user, Message message)
    {
        // 将消息转小写以进行不区分大小写的关键词匹配
        var msgLower = message.message.ToLower();
        // 将消息解析为HTML形式
        var text = _client.EntitiesToHtml(message.message, message.entities);

        // 获取所有匹配的关键词列表
        var matchedKeywords = Utils.GetMatchingKeywords(msgLower, _keywords);
        if (matchedKeywords.Count == 0) return; // 无匹配关键词则直接返回

        // 将_data中的内容格式化为HTML加粗文本
        string formattedData = string.Join(Environment.NewLine, _data.Select(line => $"<b>{line}</b>"));

        // 构造关键词显示字符串
        var allKeywordDisplays = string.Join(", ", matchedKeywords.Select(k => "#" + k.Replace("?", "")));

        // 构建包含所有匹配关键词信息的消息
        var msg =
            $"<b>命中的关键词有：</b>{allKeywordDisplays}{Environment.NewLine}{Environment.NewLine}" +
            $"用户ID:<code>{user.id}</code>{Environment.NewLine}" +
            $"用户:{GetTelegramUserLink(user)}\t{GetTelegramUserName(user)}{Environment.NewLine}" +
            $"来自于:<code>【{channel.title}】</code>\t@{channel.MainUsername ?? ""}{Environment.NewLine}" +
            $"捕捉时间:<code>{message.Date.AddHours(8):yyyy年MM月dd日HH时mm分ss秒}</code>{Environment.NewLine}" +
            $"发送内容:{Environment.NewLine}<b>{text}</b>{Environment.NewLine}{Environment.NewLine}" +
            $"消息链接:<a href=\"https://t.me/{channel.MainUsername ?? ("c/" + channel.id)}/{message.id}\">【点击直达】</a>{Environment.NewLine}{Environment.NewLine}" +
            $"{formattedData}{Environment.NewLine}";

        // 将HTML转换为Telegram可用的实体格式
        var entities = _client.HtmlToEntities(ref msg, users: _manager?.Users);

        try
        {
            var chat = Chat(_sendChatId);
            if (chat != null)
            {
                await _client.SendMessageAsync(chat, msg,
                   preview: Client.LinkPreview.Disabled,
                   entities: entities,
                   media: message.media?.ToInputMedia());
            }
            else
            {
                Utils.Log("发送消息时发生异常: 无法找到指定的聊天对象。");
            }
        }
        catch (Exception ex)
        {
            Utils.Log($"发送消息时发生异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户的所有用户名（如 @username）并以空格分隔，若无则返回空字符串。
    /// </summary>
    private string GetTelegramUserName(User user)
    {
        var userNames = user.ActiveUsernames?.Select(u => $"@{u}").ToArray() ?? Array.Empty<string>();
        return userNames.Length > 0 ? string.Join(" ", userNames) : string.Empty;
    }

    /// <summary>
    /// 获取用户的昵称（组合 first_name 和 last_name），并去除 HTML 特殊字符。
    /// 若用户无名称，则返回空字符串。
    /// </summary>
    private string GetTelegramNickName(User user)
    {
        var nickName = (user.first_name + user.last_name)?.Replace("<", "").Replace(">", "");
        return nickName ?? string.Empty;
    }

    /// <summary>
    /// 获取用户的超链接。若用户有昵称则展示昵称，否则显示用户ID。
    /// 点击链接可直接在 Telegram 中跳转到该用户。
    /// </summary>
    private string GetTelegramUserLink(User user)
    {
        var nickName = GetTelegramNickName(user);
        var displayName = string.IsNullOrEmpty(nickName) ? user.id.ToString() : nickName;
        return $"<a href=\"tg://user?id={user.id}\">{displayName}</a>";
    }

    /// <summary>
    /// 尝试从消息中获取有效的用户和频道对象。若不符合条件则返回 false。
    /// </summary>
    /// <param name="message">要验证的消息对象</param>
    /// <param name="channel">输出的频道对象，如果验证通过</param>
    /// <param name="user">输出的用户对象，如果验证通过</param>
    /// <returns>如果消息有效则返回 true，否则返回 false</returns>
    private bool TryGetValidChannelAndUser(Message message, out Channel? channel, out User? user)
    {
        channel = null;
        user = null;

        // 检查管理器是否初始化
        if (_manager == null)
        {
            Utils.Log("更新管理器未初始化。");
            return false;
        }

        // 检查消息发送者 ID 是否存在
        if (message.from_id == null)
        {
            Utils.Log("消息发送者 ID 为空。");
            return false;
        }

        // 使用辅助方法获取用户对象
        user = User(message.from_id);
        if (user == null)
        {
            Utils.Log($"未找到用户 ID: {message.from_id}");
            return false;
        }

        // 忽略 Bot 用户的消息
        if (user.IsBot)
        {
            Utils.Log($"忽略 Bot 用户消息: {user.id}");
            return false;
        }

        // 使用辅助方法获取聊天对象
        var chatBase = Chat(message.peer_id);
        if (chatBase == null)
        {
            Utils.Log($"未找到聊天对象 ID: {message.peer_id}");
            return false;
        }

        // 仅处理群组聊天
        if (!chatBase.IsGroup)
        {
            Utils.Log($"聊天对象不是群组: {chatBase}");
            return false;
        }

        // 检查消息内容是否为空或仅包含空白字符
        if (string.IsNullOrWhiteSpace(message.message))
        {
            Utils.Log("消息内容为空或仅包含空白字符。");
            return false;
        }

        // 尝试将 ChatBase 转换为 Channel 类型
        channel = chatBase as Channel;
        if (channel == null)
        {
            Utils.Log($"聊天对象不是 Channel 类型: {chatBase}");
            return false;
        }

        return true;
    }
}