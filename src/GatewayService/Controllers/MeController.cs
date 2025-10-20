using GatewayService.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/v1/me")]
public class MeController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<MeController> _logger;

    public MeController(IGatewayService gatewayService, ILogger<MeController> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserInfo([FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        try
        {
            var userInfo = await _gatewayService.GetUserInfoAsync(username);
            return Ok(userInfo);
        }
        catch (ServiceUnavailableException ex)
        {
            _logger.LogWarning(ex, "Critical service unavailable for user: {Username}", username);
            return StatusCode(503, new { message = "Service unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for: {Username}", username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}