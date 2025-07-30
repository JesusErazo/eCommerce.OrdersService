using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
  public static readonly ResiliencePropertyKey<IEnumerable<Guid>> ProductIDsKey = new("productIDs");
  private readonly ILogger<ProductsMicroservicePolicies> _logger;
  public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
  {
    _logger = logger;
  }
  public ResiliencePipeline<HttpResponseMessage> GetFallbackPolicy()
  {

    return new ResiliencePipelineBuilder<HttpResponseMessage>()
      .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>()
      {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult(r => !r.IsSuccessStatusCode),
        FallbackAction = args =>
        {
          _logger.LogWarning("Fallback triggered: The request failed, returning dummy data.");

          HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

          if (args.Context.Properties.TryGetValue(ProductIDsKey, out IEnumerable<Guid>? productIDs) &&
              productIDs is not null
          )
          {
            List<ProductResponse> productsDummyData = productIDs
            .Select(id => new ProductResponse(
              ProductID: id,
              ProductName: "Temporarily Unavailable",
              Category: "Temporarily Unavailable",
              UnitPrice: 0,
              QuantityInStock: 0
              ))
            .ToList();

            response.Content = new StringContent(JsonSerializer.Serialize(productsDummyData), Encoding.UTF8, "application/json");
          }
          else
          {
            response.Content = new StringContent("[]", Encoding.UTF8, "application/json");
          }

          return Outcome.FromResultAsValueTask(response);
        }
      })
      .Build();
  }
}
