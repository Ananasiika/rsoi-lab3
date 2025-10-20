using GatewayService.Models;
using System.Text;
using System.Text.Json;
using GatewayService.Dto;

namespace GatewayService.HttpClients;

public class TicketClient : ITicketClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TicketClient> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public TicketClient(HttpClient httpClient, ILogger<TicketClient> logger, CircuitBreaker circuitBreaker)
    {
        _httpClient = httpClient;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        
        _circuitBreaker.RegisterHealthCheck("TicketService", HealthCheckAsync);
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

    public async Task<ServiceResponse<List<TicketResponse>>> GetUserTicketsAsync(string username)
    {
        return await _circuitBreaker.ExecuteAsync(
            "TicketService",
            async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                request.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<TicketResponse>();
                    return ServiceResponse<List<TicketResponse>>.Success(tickets);
                }
                
                return ServiceResponse<List<TicketResponse>>.ErrorResponse(
                    $"Failed to get tickets: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<List<TicketResponse>>.ServiceUnavailable("Ticket"));
    }

    public async Task<ServiceResponse<TicketResponse?>> GetTicketAsync(string username, Guid ticketUid)
    {
        return await _circuitBreaker.ExecuteAsync(
            "TicketService",
            async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var ticket = JsonSerializer.Deserialize<TicketResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ServiceResponse<TicketResponse?>.Success(ticket);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse<TicketResponse?>.Success(null);
                }
                
                return ServiceResponse<TicketResponse?>.ErrorResponse(
                    $"Failed to get ticket: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<TicketResponse?>.ServiceUnavailable("Ticket"));
    }

    public async Task<ServiceResponse<TicketPurchaseResponse?>> PurchaseTicketAsync(string username, TicketPurchaseRequest request)
    {
        return await _circuitBreaker.ExecuteAsync(
            "TicketService",
            async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
                {
                    Content = content
                };
                httpRequest.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(httpRequest);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var ticketResponse = JsonSerializer.Deserialize<TicketPurchaseResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ServiceResponse<TicketPurchaseResponse?>.Success(ticketResponse);
                }
                
                return ServiceResponse<TicketPurchaseResponse?>.ErrorResponse(
                    $"Failed to purchase ticket: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<TicketPurchaseResponse?>.ServiceUnavailable("Ticket"));
    }

    public async Task<ServiceResponse<bool>> CancelTicketAsync(string username, Guid ticketUid)
    {
        return await _circuitBreaker.ExecuteAsync(
            "TicketService",
            async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return ServiceResponse<bool>.Success(true);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ServiceResponse<bool>.Success(false);
                }
                
                return ServiceResponse<bool>.ErrorResponse(
                    $"Failed to cancel ticket: {response.StatusCode}", 
                    (int)response.StatusCode);
            },
            ServiceResponse<bool>.ServiceUnavailable("Ticket"));
    }
}