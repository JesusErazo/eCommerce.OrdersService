using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
  private readonly HttpClient _httpClient;
  public ProductsMicroserviceClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<ProductResponse?> GetProductByID(Guid productID)
  {
    HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");

    if (!response.IsSuccessStatusCode)
    {
      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return null;
      }else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        throw new HttpRequestException("Bad request",null, System.Net.HttpStatusCode.BadRequest);
      }
      else
      {
        throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
      }
    }

    ProductResponse? existingProduct = await response.Content.ReadFromJsonAsync<ProductResponse>();

    if (existingProduct is null) {
      throw new ArgumentException("Invalid Product ID");
    }

    return existingProduct;
  }
}
