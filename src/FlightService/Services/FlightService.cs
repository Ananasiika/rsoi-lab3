using FlightService.Database;
using FlightService.Interfaces;
using FlightService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Services;

public class FlightService : IFlightService
{
    private readonly FlightDatabaseContext _context;

    public FlightService(FlightDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Flight>> GetAllFlightsAsync(int page, int size)
    {
        return await _context.Flights
            .Include(f => f.FromAirport)
            .Include(f => f.ToAirport)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<Flight?> GetFlightByIdAsync(int id)
    {
        return await _context.Flights
            .Include(f => f.FromAirport)
            .Include(f => f.ToAirport)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Flights.CountAsync();
    }

    public async Task<Flight?> GetFlightByNumberAsync(string flightNumber)
    {
        return await _context.Flights
            .Include(f => f.FromAirport)
            .Include(f => f.ToAirport)
            .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
    }

    public async Task<Flight> CreateFlightAsync(Flight flight)
    {
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();
        return flight;
    }

    public async Task<bool> FlightExistsAsync(string flightNumber)
    {
        return await _context.Flights.AnyAsync(f => f.FlightNumber == flightNumber);
    }
}