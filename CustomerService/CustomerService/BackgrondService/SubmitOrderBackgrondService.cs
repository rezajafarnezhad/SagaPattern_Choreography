using CustomerService.Service;

namespace CustomerService.BackgrondService;

public class SubmitOrderBackgrondService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public SubmitOrderBackgrondService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();
            await service.HandleSubmitOrder();
        }
    }
}