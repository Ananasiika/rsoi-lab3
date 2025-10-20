using GatewayService.Dto;
using GatewayService.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayService.HttpClients;

public class FlightClient : IFlightClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlightClient> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public FlightClient(HttpClient httpClient, ILogger<FlightClient> logger, CircuitBreaker circuitBreaker)
    {
        _httpClient = httpClient;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
    }

    public async Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size)
    {
        return await _circuitBreaker.ExecuteAsync(
            "FlightService",
            async () =>
            {
                var response = await _httpClient.GetAsync($"/api/v1/flights?page={page}&size={size}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<PaginationResponse<FlightDto>>(content, options) ?? new PaginationResponse<FlightDto>();
                }

                _logger.LogWarning("Failed to get flights. Status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to get flights: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for GetFlightsAsync");
                return new PaginationResponse<FlightDto>();
            });
    }

    public async Task<FlightDto?> GetFlightByNumberAsync(string flightNumber)
    {
        return await _circuitBreaker.ExecuteAsync(
            "FlightService",
            async () =>
            {
                var response = await _httpClient.GetAsync($"/api/v1/flights/number/{flightNumber}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    return JsonSerializer.Deserialize<FlightDto>(content, options);
                }

                _logger.LogWarning("Flight not found: {FlightNumber}", flightNumber);
                throw new HttpRequestException($"Flight not found: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for GetFlightByNumberAsync: {FlightNumber}", flightNumber);
                return null;
            });
    }
}