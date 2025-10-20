using FlightService.Models;

namespace FlightService.Interfaces;

public interface IAirportService
{
    Task<IEnumerable<Airport>> GetAllAirportsAsync();

    Task<Airport?> GetAirportByIdAsync(int id);

    Task<Airport> CreateAirportAsync(Airport airport);
}