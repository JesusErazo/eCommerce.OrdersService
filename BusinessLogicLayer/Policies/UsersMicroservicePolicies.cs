using eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace eCommerce.OrdersService.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
  private readonly ILogger<UsersMicroservicePolicies> _logger;
  public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
  {
    _logger = logger;
  }

  public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    AsyncRetryPolicy<HttpResponseMessage> policy =
    Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(
      retryCount: 3,
      sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
      onRetry: (outcome, timespan, retryAttempt, context) =>
      {
        _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
      }
    );

    return policy;
  }
}
