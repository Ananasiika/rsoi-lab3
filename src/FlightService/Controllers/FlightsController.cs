using FlightService.Interfaces;
using FlightService.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightService.Controllers;

[ApiController]
[Route("api/v1/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<FlightDto>>> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1)
        {
            return BadRequest("Page and size must be positive integers");
        }

        var flights = await _flightService.GetAllFlightsAsync(page, size);
        var totalCount = await _flightService.GetTotalCountAsync();

        // Преобразуем Flight в FlightDto
        var flightDtos = flights.Select(f => new FlightDto
        {
            FlightNumber = f.FlightNumber,
            FromAirport = f.FromAirport,
            ToAirport = f.ToAirport,
            Date = f.DateTime,
            Price = f.Price
        }).ToList();

        var response = new PaginationResponse<FlightDto>
        {
            Page = page,
            PageSize = size,
            TotalElements = totalCount, // Измените на TotalElements
            Items = flightDtos
        };

        return Ok(response);
    }

    [HttpGet("number/{flightNumber}")]
    public async Task<ActionResult<FlightDto>> GetFlightByNumber(string flightNumber)
    {
        var flight = await _flightService.GetFlightByNumberAsync(flightNumber);
        if (flight == null)
        {
            return NotFound();
        }

        var flightDto = new FlightDto
        {
            FlightNumber = flight.FlightNumber,
            FromAirport = flight.FromAirport,
            ToAirport = flight.ToAirport,
            Date = flight.DateTime,
            Price = flight.Price
        };

        return Ok(flightDto);
    }

    [HttpPost]
    public async Task<ActionResult<Flight>> CreateFlight(Flight flight)
    {
        try
        {
            var createdFlight = await _flightService.CreateFlightAsync(flight);
            return CreatedAtAction(nameof(CreateFlight), new { id = createdFlight.Id }, createdFlight);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating flight: {ex.Message}");
        }
    }
}