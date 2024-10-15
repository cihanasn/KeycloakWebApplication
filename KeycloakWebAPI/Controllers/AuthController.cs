﻿using KeycloakWebAPI.Dtos;
using KeycloakWebAPI.Options;
using KeycloakWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace KeycloakWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController(
    IOptions<KeycloakConfiguration> options,
    KeycloakService keycloakService) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDto request, CancellationToken cancellationToken = default)
    {
        string endpoint = $"{options.Value.HostName}/admin/realms/{options.Value.Realm}/users";
        object data = new
        {
            username = request.UserName,
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            enabled = true,
            emailVerified = true,
            credentials = new List<object>()
            {
                new {
                    type = "password",
                    temporary = false,
                    value = request.Password
                }
            }
        };

        string stringData = JsonSerializer.Serialize(data);
        var content = new StringContent(stringData, Encoding.UTF8, "application/json");

        HttpClient httpClient = new();

        string token = await keycloakService.GetAccessToken();

        //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var message = await httpClient.PostAsync(endpoint, content, cancellationToken);

        if (!message.IsSuccessStatusCode)
        {
            var response = await message.Content.ReadAsStringAsync();

            if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorResultForUnauthorized = JsonSerializer.Deserialize<ErrorResponseDto>(response);

                return Unauthorized(new { ErrorMessage = errorResultForUnauthorized!.ErrorDescription });
            }
            else 
            {
                var errorResultForOthers = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(response);

                return BadRequest(new { ErrorMessage = errorResultForOthers!.ErrorMessage });
            }

            
        }

        return Ok(new { Message = "User was created successfully." });

    }
}
