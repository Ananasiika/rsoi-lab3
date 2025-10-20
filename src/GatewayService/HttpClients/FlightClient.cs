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
        
        _circuitBreaker.RegisterHealthCheck("FlightService", HealthCheckAsync);
    }

    private async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/manage/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ServiceResponse<PaginationResponse<FlightDto>>> GetFlightsAsync(int page, int size)
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
                    var flights = JsonSerializer.Deserialize<PaginationResponse<FlightDto>>(content, options) 
                        ?? new PaginationResponse<FlightDto>();
                    return ServiceResponse<PaginationResponse<FlightDto>>.Success(flights);
                }
            
                return ServiceResponse<PaginationResponse<FlightDto>>.ErrorResponse(
                    $"Failed to get flights: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<PaginationResponse<FlightDto>>.ServiceUnavailable("Flight"));
    }

    public async Task<ServiceResponse<FlightDto?>> GetFlightByNumberAsync(string flightNumber)
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
                
                    var flight = JsonSerializer.Deserialize<FlightDto>(content, options);
                    return ServiceResponse<FlightDto?>.Success(flight);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse<FlightDto?>.Success(null);
                }
                
                return ServiceResponse<FlightDto?>.ErrorResponse(
                    $"Failed to get flight: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<FlightDto?>.ServiceUnavailable("Flight"));
    }
}