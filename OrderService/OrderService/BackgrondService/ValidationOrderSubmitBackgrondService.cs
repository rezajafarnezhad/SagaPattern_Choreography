using OrderService.OrderService;

namespace OrderService.BackgrondService;

public class ValidationOrderSubmitBackgrondService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public ValidationOrderSubmitBackgrondService(IServiceScopeFactory scopeFactory)
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
            var service = scope.ServiceProvider.GetRequiredService<IOrderService>();
            await service.HandleValidationOrder();
        }
    }
}