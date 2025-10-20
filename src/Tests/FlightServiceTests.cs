using FlightService.Controllers;
using FlightService.Database;
using FlightService.Models;
using FlightService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public class FlightServiceTests : IDisposable
{
    private readonly FlightDatabaseContext _context;
    private readonly FlightsController _flightsController;
    private readonly AirportsController _airportsController;

    public FlightServiceTests()
    {
        _context = DbHelper.CreateContext<FlightDatabaseContext>();

        var flightService = new FlightService.Services.FlightService(_context);
        var airportService = new AirportService(_context);

        _flightsController = new FlightsController(flightService);
        _airportsController = new AirportsController(airportService);

        SeedData();
    }

    private void SeedData()
    {
        // Clear existing data first
        _context.Airports.RemoveRange(_context.Airports);
        _context.Flights.RemoveRange(_context.Flights);
        _context.SaveChanges();

        // Add airports
        var airports = new List<Airport>
        {
            new Airport { Id = 1, Name = "Sheremetyevo", City = "Moscow", Country = "Russia" },
            new Airport { Id = 2, Name = "Pulkovo", City = "Saint Petersburg", Country = "Russia" },
            new Airport { Id = 3, Name = "Vnukovo", City = "Moscow", Country = "Russia" }
        };

        // Add flights
        var flights = new List<Flight>
        {
            new() {
                Id = 1,
                FlightNumber = "FL123",
                FromAirportId = 1,
                ToAirportId = 2,
                DateTime = DateTime.UtcNow.AddDays(1),
                Price = 5000
            },
            new()
            {
                Id = 2,
                FlightNumber = "FL456",
                FromAirportId = 2,
                ToAirportId = 1,
                DateTime = DateTime.UtcNow.AddDays(2),
                Price = 4500
            },
            new Flight
            {
                Id = 3,
                FlightNumber = "FL789",
                FromAirportId = 1,
                ToAirportId = 3,
                DateTime = DateTime.UtcNow.AddDays(3),
                Price = 3000
            }
        };

        _context.Airports.AddRange(airports);
        _context.Flights.AddRange(flights);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task CreateFlight_ValidFlight_ReturnsCreated()
    {
        // Arrange
        var newFlight = new Flight
        {
            FlightNumber = "FL999",
            FromAirportId = 1,
            ToAirportId = 2,
            DateTime = DateTime.UtcNow.AddDays(5),
            Price = 6000
        };

        // Act
        var result = await _flightsController.CreateFlight(newFlight);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Flight>>(result);
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var flight = Assert.IsType<Flight>(createdAtResult.Value);

        Assert.Equal("FL999", flight.FlightNumber);

        // Verify it was actually saved
        var savedFlight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "FL999");
        Assert.NotNull(savedFlight);
        Assert.Equal(6000, savedFlight.Price);
    }

    [Fact]
    public async Task GetAirports_ReturnsOk()
    {
        // Act
        var result = await _airportsController.GetAirports();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Airport>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var airports = Assert.IsType<List<Airport>>(okResult.Value);

        Assert.Equal(3, airports.Count);
        Assert.Equal("Sheremetyevo", airports.First().Name);
    }

    [Fact]
    public async Task GetAirport_ExistingId_ReturnsOk()
    {
        // Act
        var result = await _airportsController.GetAirport(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Airport>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var airport = Assert.IsType<Airport>(okResult.Value);

        Assert.Equal(1, airport.Id);
        Assert.Equal("Sheremetyevo", airport.Name);
        Assert.Equal("Moscow", airport.City);
    }

    [Fact]
    public async Task GetAirport_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _airportsController.GetAirport(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Airport>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateAirport_ValidAirport_ReturnsCreated()
    {
        // Arrange
        var newAirport = new Airport
        {
            Name = "Domodedovo",
            City = "Moscow",
            Country = "Russia"
        };

        // Act
        var result = await _airportsController.CreateAirport(newAirport);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Airport>>(result);
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var airport = Assert.IsType<Airport>(createdAtResult.Value);

        Assert.Equal("Domodedovo", airport.Name);

        // Verify it was actually saved
        var savedAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Name == "Domodedovo");
        Assert.NotNull(savedAirport);
        Assert.Equal("Moscow", savedAirport.City);
    }
}