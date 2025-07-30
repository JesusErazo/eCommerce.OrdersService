using eCommerce.OrdersService.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net.Http.Json;

namespace eCommerce.OrdersService.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<UsersMicroserviceClient> _logger;

  public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
  {
    _httpClient = httpClient;
    _logger = logger;
  }

  public async Task<UserResponse?> GetUserByUserID(Guid userID)
  {
    try
    {
      HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

      if (!response.IsSuccessStatusCode)
      {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          return null;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
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

      if (userResponse == null)
      {
        throw new ArgumentException("Invalid User ID");
      }

      return userResponse;
    }
    catch (BrokenCircuitException ex)
    {
      _logger.LogInformation(ex, "Request failed because of circuit breaker is in open state. Returning dummy data.");

      return new UserResponse
          (
            UserID: Guid.Empty,
            Email: "Temporarily Unavailable",
            PersonName: "Temporarily Unavailable",
            Gender: "Temporarily Unavailable"
          );
    }
  }

  public async Task<IEnumerable<UserResponse?>> GetUsersByUserIDs(Guid[] userIDs)
  {
    try
    {
      string queryStrings = string.Join("&ids=", userIDs);
      string url = $"/api/users/search?ids={queryStrings}";

      HttpResponseMessage response = await _httpClient.GetAsync(url);

      if (!response.IsSuccessStatusCode)
      {
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
          throw new HttpRequestException("At least one User ID is required");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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
    catch (BrokenCircuitException ex)
    {
      _logger.LogInformation(ex, "Request failed because of circuit breaker is in open state. Returning dummy data.");

      int dummyUsersAmount = userIDs.Length;

      List<UserResponse> dummyUsers = new List<UserResponse>();

      foreach (Guid userID in userIDs)
      {
        dummyUsers.Add(
          new UserResponse(
            UserID: userID,
            Email: "Temporarily Unavailable",
            PersonName: "Temporarily Unavailable",
            Gender: "Temporarily Unavailable"
          )
        );
      }

      return dummyUsers;
    }
  }
}
