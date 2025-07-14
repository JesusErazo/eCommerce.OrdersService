using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
  private readonly HttpClient _httpClient;

  public UsersMicroserviceClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<UserResponse?> GetUserByUserID(Guid userID)
  {
    HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

    if (!response.IsSuccessStatusCode)
    {
      if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return null;
      }else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
      }
      else
      {
        throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
      }
    }

    UserResponse? userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();

    if (userResponse == null) {
      throw new ArgumentException("Invalid User ID");
    }

    return userResponse;
  }
}
