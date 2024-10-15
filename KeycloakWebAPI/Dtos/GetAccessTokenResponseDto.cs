using System.Text.Json.Serialization;

namespace KeycloakWebAPI.Dtos;

public sealed class GetAccessTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;
}
