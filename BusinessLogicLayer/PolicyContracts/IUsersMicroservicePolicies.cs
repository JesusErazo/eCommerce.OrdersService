using Polly;

namespace eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;

public interface IUsersMicroservicePolicies
{
  IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
}
