using KeycloakWebAPI.Dtos;
using KeycloakWebAPI.Options;
using KeycloakWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KeycloakWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize("users")]
    public class UsersController(
    KeycloakService keycloakService,
    IOptions<KeycloakConfiguration> options) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            string enpoint = $"{options.Value.HostName}/admin/realms/{options.Value.Realm}/users";

            var response = await keycloakService.GetAsync<List<UserDto>>(enpoint, true, cancellationToken);

            return StatusCode(response.StatusCode, response);
        }
    }
}
