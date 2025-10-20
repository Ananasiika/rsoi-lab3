using FlightService.Models;

namespace FlightService.Interfaces;

public interface IFlightService
{
    Task<IEnumerable<Flight>> GetAllFlightsAsync(int page, int size);

    Task<Flight?> GetFlightByIdAsync(int id);

    Task<Flight?> GetFlightByNumberAsync(string flightNumber);

    Task<Flight> CreateFlightAsync(Flight flight);

    Task<bool> FlightExistsAsync(string flightNumber);

    Task<int> GetTotalCountAsync();
}