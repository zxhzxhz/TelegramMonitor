using System.Text;
using TelegramMonitor;
using WTelegram;

try
{
    Utils.Log("Hello, This is By @Riniba!");

    // 循环请求用户输入手机号，直到格式合法
    string phoneNumber = Utils.PromptForPhoneNumber();
    Utils.Log("正在登录请稍候...");

    // 改进日志初始化，确保资源正确释放
    await using StreamWriter WTelegramLogs = new(Constants.LOG_FILE_PATH, true, Encoding.UTF8) { AutoFlush = true };
    Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

    // 使用登录的手机号创建对应的Session文件名
    using WTelegram.Client client = new WTelegram.Client(Constants.TELEGRAM_API_ID, Constants.TELEGRAM_API_HASH, $"{phoneNumber}.session");

    // 创建Telegram工具类实例进行后续登录及其他操作
    TelegramServer telegramServer = new TelegramServer(client);

    // 尝试登录并捕获可能的异常
    await telegramServer.DoLoginAsync(phoneNumber);
}
catch (Exception ex)
{
    Utils.Log($"程序运行出错：{ex.Message}");
    Utils.Log($"详细信息：{ex}");
}