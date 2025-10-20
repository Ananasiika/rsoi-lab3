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
        if (page < 1 || size < 1 || size > 100)
        {
            return BadRequest(new { message = "Invalid page or size parameters" });
        }

        var response = await _gatewayService.GetFlightsAsync(page, size);
        
        if (response.IsSuccess)
        {
            return Ok(response.Response);
        }
        
        var errorMessage = response.Error?.Message ?? "Service error";
        return StatusCode(response.StatusCode, new { message = errorMessage });
    }
}