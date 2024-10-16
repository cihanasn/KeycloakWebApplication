using KeycloakWebAPI.Dtos;
using KeycloakWebAPI.Options;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.Net;

namespace KeycloakWebAPI.Services;

public sealed class KeycloakService(
    IOptions<KeycloakConfiguration> options)
{
    public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
    {
        HttpClient client = new();

        string endpoint = $"{options.Value.HostName}/realms/{options.Value.Realm}/protocol/openid-connect/token";

        List<KeyValuePair<string, string>> data = new();

        KeyValuePair<string, string> grantType = new("grant_type", "client_credentials");
        KeyValuePair<string, string> clientId = new("client_id", options.Value.ClientId);
        KeyValuePair<string, string> clientSecret = new("client_secret", options.Value.ClientSecret);

        data.Add(grantType);
        data.Add(clientId);
        data.Add(clientSecret);

        //var message = await client.PostAsync(endpoint, new FormUrlEncodedContent(data), cancellationToken);

        //var response = await message.Content.ReadAsStringAsync();

        //if (!message.IsSuccessStatusCode)
        //{

        //    if (message.StatusCode == System.Net.HttpStatusCode.BadRequest)
        //    {
        //        var errorResultForBadRequest = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(response);

        //        throw new ArgumentException(errorResultForBadRequest!.ErrorMessage);
        //    }

        //    var errorResultForOther = JsonSerializer.Deserialize<ErrorResponseDto>(response);

        //    throw new ArgumentException(errorResultForOther!.ErrorDescription);
        //}

        //var result = JsonSerializer.Deserialize<GetAccessTokenResponseDto>(response);

        Result<GetAccessTokenResponseDto> result = await PostAsyncForFormUrlEncodedContent<GetAccessTokenResponseDto>(endpoint, data, false, cancellationToken);

        return result.Value!.AccessToken;
    }

    public async Task<Result<T>> PostAsync<T>(string endpoint, object data, bool isTokenRequired = false, CancellationToken cancellationToken = default)
    {
        string stringData = JsonSerializer.Serialize(data);
        var content = new StringContent(stringData, Encoding.UTF8, "application/json");

        HttpClient httpClient = new();

        if(isTokenRequired) 
        {
            string token = await GetAccessToken();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        var message = await httpClient.PostAsync(endpoint, content, cancellationToken);

        var response = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorResultForUnauthorized = JsonSerializer.Deserialize<ErrorResponseDto>(response);
                return Result<T>.Failure(errorResultForUnauthorized!.ErrorDescription, null, (int)message.StatusCode);
            }
            else
            {
                var errorResultForOthers = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(response);
                return Result<T>.Failure(errorResultForOthers!.ErrorMessage, null, (int)message.StatusCode);
            }
        }
        if(message.StatusCode == HttpStatusCode.Created || message.StatusCode == HttpStatusCode.NoContent)
        {
            return Result<T>.Success(default!, null, (int)message.StatusCode);
        }


        var obj = JsonSerializer.Deserialize<T>(response);

        return Result<T>.Success(obj!, null, (int)message.StatusCode);
    }

    public async Task<Result<T>> PostAsyncForFormUrlEncodedContent<T>(string endpoint, List<KeyValuePair<string, string>> data, bool isTokenRequired = false, CancellationToken cancellationToken = default)
    {

        HttpClient httpClient = new();

        if (isTokenRequired)
        {
            string token = await GetAccessToken();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        var message = await httpClient.PostAsync(endpoint, new FormUrlEncodedContent(data), cancellationToken);

        var response = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorResultForUnauthorized = JsonSerializer.Deserialize<ErrorResponseDto>(response);
                return Result<T>.Failure(errorResultForUnauthorized!.ErrorDescription, null, (int)message.StatusCode);
            }
            else
            {
                var errorResultForOthers = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(response);
                return Result<T>.Failure(errorResultForOthers!.ErrorMessage, null, (int)message.StatusCode);
            }
        }
        if (message.StatusCode == HttpStatusCode.Created || message.StatusCode == HttpStatusCode.NoContent)
        {
            return Result<T>.Success(default!, null, (int)message.StatusCode);
        }


        var obj = JsonSerializer.Deserialize<T>(response);

        return Result<T>.Success(obj!);
    }
}
