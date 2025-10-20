using GatewayService.HttpClients;
using GatewayService.Models;
using System.Collections.Concurrent;

namespace GatewayService;

public class RetryItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string OperationType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
}

public interface IRetryQueue
{
    void Enqueue(RetryItem item);

    Task ProcessQueueAsync(CancellationToken cancellationToken = default);
}

public class RetryQueue : IRetryQueue, IHostedService
{
    private readonly ConcurrentQueue<RetryItem> _queue = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetryQueue> _logger;
    private Timer? _timer;

    public RetryQueue(IServiceProvider serviceProvider, ILogger<RetryQueue> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Enqueue(RetryItem item)
    {
        _queue.Enqueue(item);
        _logger.LogInformation("Item enqueued for retry: {OperationType} for user {Username}",
            item.OperationType, item.Username);
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        var itemsToRetry = new List<RetryItem>();

        while (_queue.TryDequeue(out var item))
        {
            if (item.RetryCount >= 3 || DateTime.UtcNow - item.CreatedAt > TimeSpan.FromSeconds(10))
            {
                _logger.LogWarning("Retry item expired or max retries exceeded: {Id}", item.Id);
                continue;
            }

            itemsToRetry.Add(item);
        }

        foreach (var item in itemsToRetry)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bonusClient = scope.ServiceProvider.GetRequiredService<IBonusClient>();

                bool success = false;
                switch (item.OperationType)
                {
                    case "UpdatePrivilegeAfterCancel":
                        var cancelData = System.Text.Json.JsonSerializer.Deserialize<CancelData>(
                            System.Text.Json.JsonSerializer.Serialize(item.Data));
                        if (cancelData != null)
                        {
                            await bonusClient.UpdatePrivilegeAfterCancel(item.Username, cancelData.TicketUid);
                            success = true;
                        }
                        break;

                    case "UpdatePrivilegeAfterPurchase":
                        var purchaseData = System.Text.Json.JsonSerializer.Deserialize<PurchaseData>(
                            System.Text.Json.JsonSerializer.Serialize(item.Data));
                        if (purchaseData != null)
                        {
                            await bonusClient.UpdatePrivilegeAfterPurchase(
                                purchaseData.Username,
                                purchaseData.Request,
                                purchaseData.TicketUid,
                                purchaseData.PaidByBonuses,
                                purchaseData.PaidByMoney,
                                purchaseData.BonusToAdd);
                            success = true;
                        }
                        break;
                }

                if (success)
                {
                    _logger.LogInformation("Retry operation completed successfully: {Id}", item.Id);
                }
                else
                {
                    item.RetryCount++;
                    _queue.Enqueue(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Retry operation failed: {Id}", item.Id);
                item.RetryCount++;
                _queue.Enqueue(item);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await ProcessQueueAsync(), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}

public class CancelData
{
    public Guid TicketUid { get; set; }
}

public class PurchaseData
{
    public string Username { get; set; } = string.Empty;
    public TicketPurchaseRequest Request { get; set; } = new();
    public Guid TicketUid { get; set; }
    public int PaidByBonuses { get; set; }
    public int PaidByMoney { get; set; }
    public int BonusToAdd { get; set; }
}