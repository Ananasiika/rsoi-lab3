using GatewayService.Models;
using GatewayService.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/v1/privilege")]
public class PrivilegeController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<PrivilegeController> _logger;

    public PrivilegeController(IGatewayService gatewayService, ILogger<PrivilegeController> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrivilegeInfo([FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var response = await _gatewayService.GetPrivilegeInfoAsync(username);
        
        if (response.IsSuccess)
        {
            return Ok(response.Response);
        }
        
        // Для /api/v1/privilege возвращаем 503 при недоступности BonusService
        if (response.StatusCode == 503)
        {
            return StatusCode(503, new { message = "Bonus Service unavailable" });
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }
}