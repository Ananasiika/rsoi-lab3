using BonusService.Dto;
using BonusService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BonusService.Controllers;

[ApiController]
[Route("api/v1/privilege")]
public class PrivilegeController : ControllerBase
{
    private readonly IPrivilegeService _privilegeService;

    public PrivilegeController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPrivilegeInfo([FromHeader(Name = "X-User-Name")] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        try
        {
            var privilegeInfo = await _privilegeService.GetPrivilegeInfoAsync(username);
            return Ok(privilegeInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> UpdateAfterPurchase([FromHeader(Name = "X-User-Name")] string username, [FromBody] PurchaseUpdateRequest request)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        try
        {
            await _privilegeService.ProcessPurchaseAsync(username, request);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> UpdateAfterCancel([FromHeader(Name = "X-User-Name")] string username, [FromBody] CancelUpdateRequest request)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        try
        {
            await _privilegeService.ProcessCancelAsync(username, request);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}