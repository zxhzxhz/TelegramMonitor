using Newtonsoft.Json;

namespace TelegramMonitor;

/// <summary>
/// 定期向发送HTTP请求获取广告数据。
/// </summary>
public class PeriodicHttpRequest
{
    private static readonly HttpClient _httpClient = new();
    private static readonly Timer _timer;

    static PeriodicHttpRequest()
    {
        _timer = new Timer(async _ =>
        {
            try
            {
                await Task.Run(FetchAndProcessDataAsync);
            }
            catch (Exception ex)
            {
                Utils.Log($"[Error] 定时任务执行失败: {ex.Message}\n{ex.StackTrace}");
            }
        }, null, TimeSpan.FromSeconds(Constants.POLLING_INTERVAL_SECONDS), TimeSpan.FromSeconds(Constants.POLLING_INTERVAL_SECONDS));
    }

    /// <summary>
    /// 获取并处理广告数据
    /// </summary>
    /// <returns>广告内容列表。若获取失败则返回空列表</returns>
    public static async Task<List<string>> FetchAndProcessDataAsync()
    {
        try
        {
            Utils.Log("开始获取广告数据...");
            var response = await _httpClient.GetStringAsync(Constants.MONITOR_API_ENDPOINT);
            var result = JsonConvert.DeserializeObject<HttpReturn>(response);

            if (result?.Succeeded == true && result.Data != null)
            {
                Utils.Log("广告数据获取成功");
                return result.Data;
            }

            Utils.Log("广告数据获取失败：返回数据无效");
            return new List<string>();
        }
        catch (HttpRequestException ex)
        {
            Utils.Log($"[Error] 网络请求失败: {ex.Message}");
            return new List<string>();
        }
        catch (JsonException ex)
        {
            Utils.Log($"[Error] JSON解析失败: {ex.Message}");
            return new List<string>();
        }
        catch (Exception ex)
        {
            Utils.Log($"[Error] 意外错误: {ex.Message}\n{ex.StackTrace}");
            return new List<string>();
        }
    }
}