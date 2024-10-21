namespace KeycloakWebAPI.Dtos;

public sealed record LoginDto(
    string Username,
    string Password);
