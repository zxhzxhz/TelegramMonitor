using Newtonsoft.Json;

namespace TelegramMonitor;

//Http请求工具类
public static class HttpExtensions
{
    private static readonly HttpClient _httpClient = new();

    //获取并处理广告数据
    public static async Task FetchAndProcessDataAsync()
    {
        try
        {
            LogExtensions.Debug("开始获取广告数据...");
            var response = await _httpClient.GetStringAsync(Constants.MONITOR_API_ENDPOINT);
            var result = JsonConvert.DeserializeObject<HttpReturn>(response);

            if (result?.Succeeded == true && result.Data != null)
            {
                LogExtensions.Debug("广告数据获取成功");
                Constants.DATA = result.Data;
                return;
            }

            LogExtensions.Error("广告数据获取失败：返回数据无效");
        }
        catch (HttpRequestException ex)
        {
            LogExtensions.Error($"网络请求失败: {ex.Message}");
        }
        catch (JsonException ex)
        {
            LogExtensions.Error($"JSON解析失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"意外错误: {ex.Message}\n{ex.StackTrace}");
        }
    }
}