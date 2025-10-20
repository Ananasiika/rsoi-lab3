using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface IFlightClient
{
    Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size);

    Task<FlightDto?> GetFlightByNumberAsync(string flightNumber);
}