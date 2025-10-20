using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface IFlightClient
{
    Task<ServiceResponse<PaginationResponse<FlightDto>>> GetFlightsAsync(int page, int size);
    Task<ServiceResponse<FlightDto?>> GetFlightByNumberAsync(string flightNumber);
}