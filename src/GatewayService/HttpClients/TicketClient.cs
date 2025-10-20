using GatewayService.Models;
using System.Text;
using System.Text.Json;

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
    }

    public async Task<List<TicketResponse>> GetUserTicketsAsync(string username)
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
                    return JsonSerializer.Deserialize<List<TicketResponse>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<TicketResponse>();
                }

                _logger.LogWarning("Failed to get tickets for user: {Username}", username);
                throw new HttpRequestException($"Failed to get tickets: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for GetUserTicketsAsync for user: {Username}", username);
                return new List<TicketResponse>();
            });
    }

    public async Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid)
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
                    return JsonSerializer.Deserialize<TicketResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Ticket not found: {TicketUid} for user: {Username}", ticketUid, username);
                throw new HttpRequestException($"Ticket not found: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for GetTicketAsync for user: {Username}", username);
                return null;
            });
    }

    public async Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request)
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
                    return JsonSerializer.Deserialize<TicketPurchaseResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Failed to purchase ticket for user: {Username}. Status: {StatusCode}",
                    username, response.StatusCode);
                throw new HttpRequestException($"Failed to purchase ticket: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for PurchaseTicketAsync for user: {Username}", username);
                return null;
            });
    }

    public async Task<bool> CancelTicketAsync(string username, Guid ticketUid)
    {
        return await _circuitBreaker.ExecuteAsync(
            "TicketService",
            async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to cancel ticket: {response.StatusCode}");
                }
                return true;
            },
            () =>
            {
                _logger.LogWarning("Using fallback for CancelTicketAsync for user: {Username}", username);
                return false;
            });
    }
}