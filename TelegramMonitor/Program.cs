using var cts = new CancellationTokenSource();

try
{
    await using StreamWriter WTelegramLogs = new(Constants.FilePaths.LogFile, true, Encoding.UTF8) { AutoFlush = true };
    Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

    LogExtensions.Logo();

    var phoneNumber = TelegramMonitor.StringExtensions.PromptForPhoneNumber();
    LogExtensions.Info("开始登录请稍候...");

    await using var client = new WTelegram.Client(
        Constants.TelegramConfig.ApiId,
        Constants.TelegramConfig.ApiHash,
        $"{phoneNumber}.session");

    // 应用代理配置
    ProxyExtensions.ApplyProxyToClient(client);

    var telegramServer = new TelegramManager(client);
    await telegramServer.DoLoginAsync(phoneNumber);

    LogExtensions.Warning("程序正在运行中... 输入 'stop' 并按回车键停止程序");

    await TelegramExtensions.RunMainLoopAsync(cts.Token);
}
catch (Exception ex)
{
    LogExtensions.Error($"程序运行出错：{ex.Message}");
    LogExtensions.Error($"详细信息：{ex}");
}