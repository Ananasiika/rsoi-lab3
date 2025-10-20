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

        var response = await _gatewayService.GetUserInfoAsync(username);
        
        if (response.IsSuccess)
        {
            return Ok(response.Response);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }
}