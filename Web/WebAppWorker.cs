using Microsoft.AspNetCore.Hosting;

namespace FurmAppDBot.Web;

public class WebAppWorker : BackgroundService
{
    private readonly IWebHost _webhost;

    public WebAppWorker(IWebHost webhost)
    {
        _webhost = webhost;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _webhost.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _webhost.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}