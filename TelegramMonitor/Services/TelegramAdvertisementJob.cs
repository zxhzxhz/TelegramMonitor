using Furion.Shapeless;

namespace TelegramMonitor;

[JobDetail("telegram-advertisement-job", Description = "Telegram广告监控任务", GroupName = "monitor", Concurrent = true)]
[PeriodSeconds(60, TriggerId = "telegram-ad-monitor-trigger", Description = "每分钟执行一次的广告监控任务", RunOnStart = true)]
public class TelegramAdvertisementJob : IJob
{
    private readonly ILogger<TelegramAdvertisementJob> _logger;
    private readonly IHttpRemoteService _httpRemoteService;
    private readonly SystemCacheServices _systemCacheServices;

    public TelegramAdvertisementJob(ILogger<TelegramAdvertisementJob> logger, IHttpRemoteService httpRemoteService, SystemCacheServices systemCacheServices)
    {
        _logger = logger;
        _httpRemoteService = httpRemoteService;
        _systemCacheServices = systemCacheServices;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        try
        {
            var content = await _httpRemoteService.GetAsAsync<string>(TelegramMonitorConstants.MonitorApi);
            dynamic clay = Clay.Parse(content);
            List<string> result = clay.result;
            _systemCacheServices.SetAdvertisement(result);
            _logger.LogInformation($"广告数据：{string.Join(System.Environment.NewLine, result)}");
        }
        catch (Exception e)
        {
            _logger.LogError($"获取广告数据失败：{e.Message}");
        }
    }
}