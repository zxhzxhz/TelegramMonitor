namespace TelegramMonitor;

/// <summary>
/// 代理服务类
/// </summary>
public static class ProxyExtensions
{
    /// <summary>
    /// 应用代理配置到客户端
    /// </summary>
    /// <param name="client"></param>
    public static void ApplyProxyToClient(Client client)
    {
        var config = FileExtensions.LoadProxyConfig();

        if (!config.Enabled)
        {
            LogExtensions.Info("代理未启用，使用直接连接");
            return;
        }

        switch (config.Type.ToUpper())
        {
            case "SOCKS":
                ApplySocksProxy(client, config);
                break;

            case "MTPROTO":
                ApplyMTProtoProxy(client, config);
                break;

            default:
                LogExtensions.Warning($"未知的代理类型: {config.Type}，使用直接连接");
                break;
        }
    }

    /// <summary>
    /// 配置SOCKS代理
    /// </summary>
    /// <param name="client"></param>
    /// <param name="config"></param>
    private static void ApplySocksProxy(Client client, ProxyConfig config)
    {
        try
        {
            LogExtensions.Info($"正在配置SOCKS5代理: {config.SocksHost}:{config.SocksPort}");

            client.TcpHandler = async (address, port) =>
            {
                var proxy = string.IsNullOrEmpty(config.SocksUsername) && string.IsNullOrEmpty(config.SocksPassword)
                    ? new Socks5ProxyClient(config.SocksHost, config.SocksPort)
                    : new Socks5ProxyClient(config.SocksHost, config.SocksPort, config.SocksUsername, config.SocksPassword);

                return proxy.CreateConnection(address, port);
            };

            LogExtensions.Info("SOCKS5代理配置完成");
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"配置SOCKS5代理失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 配置MTProto代理
    /// </summary>
    /// <param name="client"></param>
    /// <param name="config"></param>
    private static void ApplyMTProtoProxy(Client client, ProxyConfig config)
    {
        try
        {
            if (string.IsNullOrEmpty(config.MtprotoUrl))
            {
                LogExtensions.Warning("MTProto代理URL为空，无法配置");
                return;
            }

            LogExtensions.Info("正在配置MTProto代理");
            client.MTProxyUrl = config.MtprotoUrl;
            LogExtensions.Info("MTProto代理配置完成");
        }
        catch (Exception ex)
        {
            LogExtensions.Error($"配置MTProto代理失败: {ex.Message}");
        }
    }
}