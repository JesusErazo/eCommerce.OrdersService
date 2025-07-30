using eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace eCommerce.OrdersService.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
  private readonly ILogger<UsersMicroservicePolicies> _logger;
  public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
  {
    _logger = logger;
  }

  public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
  {
    AsyncCircuitBreakerPolicy<HttpResponseMessage> policy =
    Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .CircuitBreakerAsync(
      handledEventsAllowedBeforeBreaking: 4,
      durationOfBreak: TimeSpan.FromMinutes(2),
      onBreak: (outcome, timespan) =>
      {
        _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 4 failures. The subsequent requests will be blocked");
      },
      onReset: () =>
      {
        _logger.LogInformation("Circuit breaker closed. The subsequents requests will be allowed");
      }
    );

    return policy;
  }

  public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    AsyncRetryPolicy<HttpResponseMessage> policy =
    Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(
      retryCount: 3,
      sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
      onRetry: (outcome, timespan, retryAttempt, context) =>
      {
        _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
      }
    );

    return policy;
  }
}
