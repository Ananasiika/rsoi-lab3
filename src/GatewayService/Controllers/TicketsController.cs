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
            return BadRequest(new ErrorResponse { Message = "Username is required" });
        }

        try
        {
            var tickets = await _gatewayService.GetUserTicketsAsync(username);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for user: {Username}", username);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }

    [HttpGet("{ticketUid}")]
    public async Task<IActionResult> GetTicket([FromRoute][Required] Guid ticketUid,
        [FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new ErrorResponse { Message = "Username is required" });
        }

        try
        {
            var ticket = await _gatewayService.GetTicketAsync(username, ticketUid);
            if (ticket == null)
            {
                return NotFound(new ErrorResponse { Message = "Ticket not found" });
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket: {TicketUid} for user: {Username}", ticketUid, username);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> PurchaseTicket(
        [FromBody] TicketPurchaseRequest request,
        [FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed",
                Errors = new List<ErrorDescription>
                {
                    new() { Field = "X-User-Name", Error = "Username is required" }
                }
            });
        }

        if (request.Price <= 0)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed",
                Errors = new List<ErrorDescription>
                {
                    new() { Field = "price", Error = "Price must be positive" }
                }
            });
        }

        try
        {
            var response = await _gatewayService.PurchaseTicketAsync(username, request);
            if (response == null)
            {
                return BadRequest(new ErrorResponse { Message = "Failed to purchase ticket" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing ticket for user: {Username}", username);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }

    [HttpDelete("{ticketUid}")]
    public async Task<IActionResult> CancelTicket([FromRoute][Required] Guid ticketUid,
        [FromHeader(Name = "X-User-Name")][Required] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new ErrorResponse { Message = "Username is required" });
        }

        try
        {
            var success = await _gatewayService.CancelTicketAsync(username, ticketUid);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = "Ticket not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling ticket: {TicketUid} for user: {Username}", ticketUid, username);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }
}