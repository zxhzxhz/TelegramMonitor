namespace TelegramMonitor;

/// <summary>
/// 管理定时任务的执行
/// </summary>
public class PeriodicTaskManager : IDisposable
{
    private readonly Timer _timer;
    private bool _disposed;

    /// <summary>
    /// 初始化定时任务管理器
    /// </summary>
    public PeriodicTaskManager()
    {
        _timer = new Timer(ExecutePeriodicTaskAsync);
    }

    private async void ExecutePeriodicTaskAsync(object? state)
    {
        try
        {
            await Task.WhenAll(
                Task.Run(() => FileExtensions.LoadKeywordConfigs(Constants.FilePaths.KeywordsFile)),
                HttpExtensions.FetchAndProcessDataAsync()
            );
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"定时任务执行失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 启动定时任务
    /// </summary>
    /// <remarks>
    /// 任务将立即执行一次，然后按照配置的时间间隔重复执行
    /// </remarks>
    public void Start()
    {
        _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(Constants.SystemConfig.PollingIntervalSeconds));
        LogExtensions.Debug("定时任务已启动");
    }

    /// <summary>
    /// 停止定时任务
    /// </summary>
    public void Stop()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        LogExtensions.Error("定时任务已停止");
    }

    /// <summary>
    /// 释放定时器资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _timer.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}