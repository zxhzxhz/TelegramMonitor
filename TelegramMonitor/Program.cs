using System.Text;
using TelegramMonitor;
using WTelegram;

Utils.Log("Hello, This is By @Riniba!");

// 循环请求用户输入手机号，直到格式合法
string phoneNumber = Utils.PromptForPhoneNumber();
Utils.Log("正在登录请稍候...");

// 初始化日志到文件
using StreamWriter WTelegramLogs = new StreamWriter(Constants.LogFilePath, true, Encoding.UTF8) { AutoFlush = true };
Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

// 使用登录的手机号创建对应的Session文件名
using WTelegram.Client client = new WTelegram.Client(Constants.ApiId, Constants.ApiHash, $"{phoneNumber}.session");

// 创建Telegram工具类实例进行后续登录及其他操作
TelegramServer telegramServer = new TelegramServer(client);

// 尝试登录并捕获可能的异常
try
{
    await telegramServer.DoLoginAsync(phoneNumber);
}
catch (Exception ex)
{
    Utils.Log($"登录失败，错误原因：{ex.Message}");
}