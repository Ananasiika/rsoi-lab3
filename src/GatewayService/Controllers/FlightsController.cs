using GatewayService.Models;
using GatewayService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Controllers;

[ApiController]
[Route("api/v1/flights")]
public class FlightsController : ControllerBase
{
    private readonly IGatewayService _gatewayService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IGatewayService gatewayService, ILogger<FlightsController> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 0 || size < 1 || size > 100)
        {
            return BadRequest(new ErrorResponse { Message = "Invalid page or size parameters" });
        }

        try
        {
            var flights = await _gatewayService.GetFlightsAsync(page, size);
            var result = new PaginationResponse<FlightResponse>
            {
                Page = flights.Page,
                PageSize = flights.PageSize,
                TotalElements = flights.TotalElements,
                Items = flights.Items.Select(f => new FlightResponse
                {
                    Date = f.Date,
                    FlightNumber = f.FlightNumber,
                    FromAirport = f.FromAirport.City + " " + f.FromAirport.Name,
                    ToAirport = f.ToAirport.City + " " + f.ToAirport.Name,
                    Price = f.Price,
                }).ToList()
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights");
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }
}