using Polly;

namespace eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;

public interface IProductsMicroservicePolicies
{
  ResiliencePipeline<HttpResponseMessage> GetFallbackPolicy(); 
}
