using Newtonsoft.Json;

namespace TelegramMonitor;

//定时请求类
public class PeriodicTaskManager
{
    private readonly Timer _timer;

    public PeriodicTaskManager()
    {
        _timer = new Timer(async _ =>
        {
            try
            {
                // 载入关键词列表
                FileExtensions.LoadKeywords(Constants.KEYWORDS_FILE_PATH);
                FileExtensions.LoadBlacklistKeywords(Constants.BLACKLIST_KEYWORDS_FILE_PATH);
                FileExtensions.LoadBlacklistUsers(Constants.BLACKLIST_USERS_FILE_PATH);

                // 载入广告
                await HttpExtensions.FetchAndProcessDataAsync();
            }
            catch (Exception ex)
            {
                LogExtensions.Error($"定时任务执行失败: {ex.Message}\n{ex.StackTrace}");
            }
        }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start()
    {
        _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(Constants.POLLING_INTERVAL_SECONDS));
        LogExtensions.Debug("定时任务已启动");
    }

    public void Stop()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        LogExtensions.Error("定时任务已停止");
    }
}