using FlightService.Database;
using FlightService.Interfaces;
using FlightService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Services;

public class AirportService : IAirportService
{
    private readonly FlightDatabaseContext _context;

    public AirportService(FlightDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Airport>> GetAllAirportsAsync()
    {
        return await _context.Airports.ToListAsync();
    }

    public async Task<Airport?> GetAirportByIdAsync(int id)
    {
        return await _context.Airports.FindAsync(id);
    }

    public async Task<Airport> CreateAirportAsync(Airport airport)
    {
        _context.Airports.Add(airport);
        await _context.SaveChangesAsync();
        return airport;
    }
}