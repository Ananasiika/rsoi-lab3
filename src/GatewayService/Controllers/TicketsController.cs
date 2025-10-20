using GatewayService.Models;
using GatewayService.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/v1/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(IGatewayService gatewayService, ILogger<TicketsController> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTickets([FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var response = await _gatewayService.GetUserTicketsAsync(username);
        
        if (response.IsSuccess)
        {
            return Ok(response.Response);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }

    [HttpGet("{ticketUid}")]
    public async Task<IActionResult> GetTicket(
        [FromHeader(Name = "X-User-Name")][Required] string username,
        [FromRoute] Guid ticketUid)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var response = await _gatewayService.GetTicketAsync(username, ticketUid);
        
        if (response.IsSuccess)
        {
            if (response.Response == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }
            return Ok(response.Response);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }

    [HttpPost]
    public async Task<IActionResult> PurchaseTicket(
        [FromBody] TicketPurchaseRequest request,
        [FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var response = await _gatewayService.PurchaseTicketAsync(username, request);
        
        if (response.IsSuccess)
        {
            if (response.Response == null)
            {
                return BadRequest(new { message = "Failed to purchase ticket" });
            }
            return Ok(response.Response);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }

    [HttpDelete("{ticketUid}")]
    public async Task<IActionResult> CancelTicket(
        [FromHeader(Name = "X-User-Name")][Required] string username,
        [FromRoute] Guid ticketUid)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var response = await _gatewayService.CancelTicketAsync(username, ticketUid);
        
        if (response.IsSuccess)
        {
            return NoContent();
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }
}