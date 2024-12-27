using System.Text;
using TelegramMonitor;
using WTelegram;

try
{
    LogExtensions.Logo(); ;

    // 循环请求用户输入手机号，直到格式合法
    string phoneNumber = TelegramMonitor.StringExtensions.PromptForPhoneNumber();
    LogExtensions.Info("开始登录请稍候...");

    // 改进日志初始化，确保资源正确释放
    await using StreamWriter WTelegramLogs = new(Constants.LOG_FILE_PATH, true, Encoding.UTF8) { AutoFlush = true };
    Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

    // 使用登录的手机号创建对应的Session文件名
    using WTelegram.Client client = new WTelegram.Client(Constants.TELEGRAM_API_ID, Constants.TELEGRAM_API_HASH, $"{phoneNumber}.session");

    // 创建Telegram工具类实例进行后续登录及其他操作
    TelegramManager telegramServer = new(client);

    // 尝试登录并捕获可能的异常
    await telegramServer.DoLoginAsync(phoneNumber);

    LogExtensions.Warning("程序正在运行中... 输入 'stop' 并按回车键停止程序");

    while (true)
    {
        if (Console.ReadLine()?.ToLower() == "stop")
        {
            TelegramMonitor.LogExtensions.Error("正在停止程序...");
            break;
        }

        // 继续运行其他任务
        await Task.Delay(1000);
    }
}
catch (Exception ex)
{
    LogExtensions.Error($"程序运行出错：{ex.Message}");
    LogExtensions.Error($"详细信息：{ex}");
}