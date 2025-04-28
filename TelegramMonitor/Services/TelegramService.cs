namespace TelegramMonitor;

[ApiDescriptionSettings(Tag = "telegram", Description = "Telegram 控制接口")]
public class TelegramService : IDynamicApiController, ITransient
{
    private readonly TelegramClientManager _mgr;
    private readonly TelegramTask _task;

    public TelegramService(TelegramClientManager mgr, TelegramTask task)
    {
        _mgr = mgr;
        _task = task;
    }

    [HttpPost("login")]
    public Task<LoginState> Login([FromBody] LoginRequest req)
        => _mgr.LoginAsync(req.PhoneNumber, req.LoginInfo);

    [HttpPost("proxy")]
    public async Task<LoginState> Proxy([FromBody] ProxyRequest req)
    {
        bool wasMonitoring = _task.IsMonitoring;

        if (wasMonitoring)
        {
            await _task.StopTaskAsync();
        }

        var loginState = await _mgr.SetProxyAsync(req.Type, req.Url);

        if (loginState == LoginState.LoggedIn && wasMonitoring)
        {
            await _task.StartTaskAsync();
        }

        return loginState;
    }

    [HttpGet("status")]
    public TgStatus Status()
        => new(_mgr.IsLoggedIn, _task.IsMonitoring);

    [HttpGet("dialogs")]
    public async Task<List<DisplayDialogs>> Dialogs()
    {
        if (!_mgr.IsLoggedIn)
        {
            throw Oops.Oh("未登录");
        }
        return await _mgr.DialogsAsync();
    }

    [HttpPost("target")]
    public void Target([FromBody] long id)
    {
        if (!_mgr.IsLoggedIn) throw Oops.Oh("未登录");
        _mgr.SetSendChatId(id);
    }

    [HttpPost("start")]
    public Task<MonitorStartResult> Start()
    {
        return _task.StartTaskAsync();
    }

    [HttpPost("stop")]
    public async void Stop()
    {
        if (!_mgr.IsLoggedIn) throw Oops.Oh("未登录");
        await _task.StopTaskAsync();
    }
}

public record LoginRequest(string PhoneNumber, string LoginInfo);
public record ProxyRequest(ProxyType Type, string Url);
public record TgStatus(bool LoggedIn, bool Monitoring);