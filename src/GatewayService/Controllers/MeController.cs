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
            var userInfo = response.Response;
            if (userInfo?.Privilege != null && userInfo.Privilege.Balance == 0 && userInfo.Privilege.Status == "BRONZE")
            {
                // Заменяем privilege на пустой объект
                return Ok(new
                {
                    tickets = userInfo.Tickets,
                    privilege = "" // Пустой объект вместо {balance: 0, status: "BRONZE"}
                });
            }
            return Ok(userInfo);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }
}