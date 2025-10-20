using System.Collections.Concurrent;

namespace GatewayService;

public class CircuitBreakerState
{
    public int FailureCount { get; set; }
    public DateTime? LastFailureTime { get; set; }

    public bool IsOpen => FailureCount >= 5 && LastFailureTime.HasValue &&
                          DateTime.UtcNow - LastFailureTime.Value < TimeSpan.FromSeconds(30);
}

public class CircuitBreaker
{
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _states = new();
    private readonly ILogger<CircuitBreaker> _logger;

    public CircuitBreaker(ILogger<CircuitBreaker> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(string serviceName, Func<Task<T>> action, Func<T> fallback)
    {
        var state = _states.GetOrAdd(serviceName, _ => new CircuitBreakerState());

        if (state.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is OPEN for {ServiceName}, using fallback", serviceName);
            return fallback();
        }

        try
        {
            var result = await action();
            ResetState(serviceName);
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure(serviceName);
            _logger.LogWarning(ex, "Circuit breaker recorded failure for {ServiceName}. Failure count: {Count}",
                serviceName, state.FailureCount);

            return fallback();
        }
    }

    public async Task ExecuteAsync(string serviceName, Func<Task> action, Action fallback)
    {
        var state = _states.GetOrAdd(serviceName, _ => new CircuitBreakerState());

        if (state.IsOpen)
        {
            _logger.LogWarning("Circuit breaker is OPEN for {ServiceName}, using fallback", serviceName);
            fallback();
            return;
        }

        try
        {
            await action();
            ResetState(serviceName);
        }
        catch (Exception ex)
        {
            RecordFailure(serviceName);
            _logger.LogWarning(ex, "Circuit breaker recorded failure for {ServiceName}. Failure count: {Count}",
                serviceName, state.FailureCount);
            fallback();
        }
    }

    private void RecordFailure(string serviceName)
    {
        var state = _states.GetOrAdd(serviceName, _ => new CircuitBreakerState());
        state.FailureCount++;
        state.LastFailureTime = DateTime.UtcNow;
    }

    private void ResetState(string serviceName)
    {
        if (_states.TryGetValue(serviceName, out var state))
        {
            state.FailureCount = 0;
            state.LastFailureTime = null;
        }
    }
}