namespace TelegramMonitor;

public static class LoggingSetup
{
    public static void AddLoggingSetup(this IServiceCollection services)
    {
        services.AddMonitorLogging(options =>
        {
            options.IgnorePropertyNames = new[] { "Byte" };
            options.IgnorePropertyTypes = new[] { typeof(byte[]) };
        });

        services.AddConsoleFormatter(options =>
        {
            options.DateFormat = "yyyy-MM-dd HH:mm:ss(zzz) dddd";
            options.ColorBehavior = LoggerColorBehavior.Enabled;
        });

        ConfigureFileLogging(services);
        services.AddSqlSugarSetup();
        services.AddTelegram();
    }

    private static void ConfigureFileLogging(IServiceCollection services)
    {
        LogLevel[] logLevels = { LogLevel.Information, LogLevel.Warning, LogLevel.Error };

        foreach (var logLevel in logLevels)
        {
            services.AddFileLogging(options =>
            {
                options.WithTraceId = true;
                options.WithStackFrame = true;
                options.FileNameRule = fileName =>
                {
                    string formattedFileName = string.Format(fileName, DateTime.Now, logLevel.ToString());
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, formattedFileName);
                };
                options.WriteFilter = logMsg => logMsg.LogLevel == logLevel;
                options.HandleWriteError = writeError =>
                {
                    writeError.UseRollbackFileName(Path.GetFileNameWithoutExtension(writeError.CurrentFileName) + "-oops" + Path.GetExtension(writeError.CurrentFileName));
                };
            });
        }
    }
}