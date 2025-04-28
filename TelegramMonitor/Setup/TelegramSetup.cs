namespace TelegramMonitor;

public static class TelegramSetup
{
    public static IServiceCollection AddTelegram(this IServiceCollection services)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        if (!Directory.Exists(TelegramMonitorConstants.SessionPath))
            Directory.CreateDirectory(TelegramMonitorConstants.SessionPath);

        StreamWriter telegramLogs = new StreamWriter(Path.Combine(logDirectory, "Telegram.log"), true, Encoding.UTF8) { AutoFlush = true };

        WTelegram.Helpers.Log = (lvl, str) =>
        {
            lock (telegramLogs)
            {
                telegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
            }
        };
        services.AddSingleton<TelegramClientManager>();

        services.AddSingleton<TelegramTask>();

        return services;
    }
}