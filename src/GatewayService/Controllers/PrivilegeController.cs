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

        try
        {
            var privilegeInfo = await _gatewayService.GetPrivilegeInfoAsync(username);
            return Ok(privilegeInfo ?? new PrivilegeInfoResponse { Balance = 0, Status = "BRONZE", History = new List<BalanceHistory>() });
        }
        catch (ServiceUnavailableException)
        {
            return StatusCode(503, new { message = "Bonus service unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privilege info for: {Username}", username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}