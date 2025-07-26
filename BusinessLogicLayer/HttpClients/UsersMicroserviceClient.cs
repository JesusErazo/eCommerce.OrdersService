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
        //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
        return new UserResponse
        (
          UserID: Guid.Empty,
          Email: "Temporarily Unavailable",
          PersonName: "Temporarily Unavailable",
          Gender: "Temporarily Unavailable"
        );
      }
    }

    UserResponse? userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();

    if (userResponse == null) {
      throw new ArgumentException("Invalid User ID");
    }

    return userResponse;
  }

  public async Task<IEnumerable<UserResponse?>> GetUsersByUserIDs(Guid[] userIDs)
  {
    string queryStrings = string.Join("&ids=",userIDs);
    string url = $"/api/users/search?ids={queryStrings}";

    HttpResponseMessage response = await _httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
      if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        throw new HttpRequestException("At least one User ID is required");
      }else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return [];
      }
      else
      {
        throw new HttpRequestException("Invalid User IDs");
      }
    }

    IEnumerable<UserResponse?>? users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse?>>();

    if (users is null) return [];

    return users;
  }
}
