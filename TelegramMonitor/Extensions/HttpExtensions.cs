namespace TelegramMonitor;

/// <summary>
/// 提供 HTTP 请求相关的扩展方法
/// </summary>
public static class HttpExtensions
{
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// 异步获取并处理广告数据
    /// </summary>
    public static async Task FetchAndProcessDataAsync()
    {
        const string OperationName = "广告数据获取";

        void SetEmptyData(string errorMessage)
        {
            LogExtensions.Error($"{OperationName}: {errorMessage}");
            Constants.SystemConfig.Advertisement = [];
        }

        try
        {
            LogExtensions.Debug($"开始{OperationName}...");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var response = await _httpClient.GetStringAsync(Constants.SystemConfig.MonitorApi)
                .ConfigureAwait(false);

            var result = JsonSerializer.Deserialize<HttpReturn>(response, options);

            if (result?.Succeeded == true && result.Data != null)
            {
                LogExtensions.Debug($"{OperationName}成功");
                Constants.SystemConfig.Advertisement = result.Data;
                return;
            }

            SetEmptyData("返回数据无效");
        }
        catch (Exception ex)
        {
            var errorType = ex switch
            {
                HttpRequestException => "网络请求失败",
                JsonException => "JSON解析失败",
                _ => "意外错误"
            };

            SetEmptyData($"{errorType}: {ex.Message}");
        }
    }
}