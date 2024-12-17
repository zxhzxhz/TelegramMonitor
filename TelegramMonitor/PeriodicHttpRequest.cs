using Newtonsoft.Json;

namespace TelegramMonitor;

/// <summary>
/// 定期向发送HTTP请求获取广告数据。
/// </summary>
public class PeriodicHttpRequest
{
    private static readonly HttpClient _httpClient = new HttpClient();

    // 定时器，用于定期请求API
    private static readonly Timer _timer;

    static PeriodicHttpRequest()
    {
        // 初始化定时器，每隔指定的时间间隔执行一次请求
        _timer = new Timer(_ =>
        {
            // 使用 Task.Run 保证异步执行，不阻塞定时器回调线程
            Task.Run(async () =>
            {
                try
                {
                    await FetchAndProcessDataAsync();
                }
                catch (Exception ex)
                {
                    // 捕获定时器调用中的异常，避免程序崩溃，记录错误日志以便调试
                    Utils.Log($"[ERROR] 定时请求时发生异常: {ex.Message}");
                }
            });
        }, null, TimeSpan.FromSeconds(Constants.IntervalSeconds), TimeSpan.FromSeconds(Constants.IntervalSeconds));
    }

    /// <summary>
    /// 发起HTTP请求获取广告数据并进行处理。
    /// 若获取成功且返回JSON中的Succeeded为true，则返回数据列表，否则返回空列表。
    /// 在请求过程中出现的任何异常都会被捕获并记录到日志中。
    /// </summary>
    /// <returns>如果获取成功则返回广告内容列表，否则返回空列表</returns>
    public static async Task<List<string>> FetchAndProcessDataAsync()
    {
        try
        {
            Utils.Log("正在获取广告内容...");
            // 发起 GET 请求获取数据
            var response = await _httpClient.GetStringAsync(Constants.ApiUrl);

            // 将返回的JSON数据反序列化为HttpReturn对象
            var json = JsonConvert.DeserializeObject<HttpReturn>(response);

            // 检查数据有效性
            if (json?.Succeeded == true && json.Data != null)
            {
                Utils.Log("获取广告内容成功");
                return json.Data;
            }
            else
            {
                // 返回结果不符合期望，记录失败并返回空列表
                Utils.Log("获取广告内容失败，返回空列表");
                return new List<string>();
            }
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，返回空列表以确保调用方不会因异常而中断
            Utils.Log($"获取广告内容时发生异常: {ex.Message}");
            return new List<string>();
        }
    }
}