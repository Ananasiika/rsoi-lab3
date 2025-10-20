using GatewayService.Models;
using System.Text;
using System.Text.Json;

namespace GatewayService.HttpClients;

public class BonusClient : IBonusClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BonusClient> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public BonusClient(HttpClient httpClient, ILogger<BonusClient> logger, CircuitBreaker circuitBreaker)
    {
        _httpClient = httpClient;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
    }

    public async Task<PrivilegeInfoResponse?> GetPrivilegeInfoAsync(string username)
    {
        return await _circuitBreaker.ExecuteAsync(
            "BonusService",
            async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                request.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PrivilegeInfoResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Failed to get privilege info for user: {Username}", username);
                throw new HttpRequestException($"Failed to get privilege info: {response.StatusCode}");
            },
            () =>
            {
                _logger.LogWarning("Using fallback for GetPrivilegeInfoAsync for user: {Username}", username);
                return null;
            });
    }

    public async Task<PrivilegeShortInfo?> GetPrivilegeShortInfoAsync(string username)
    {
        var privilegeInfo = await GetPrivilegeInfoAsync(username);
        return privilegeInfo != null ? new PrivilegeShortInfo
        {
            Balance = privilegeInfo.Balance,
            Status = privilegeInfo.Status
        } : null;
    }

    public async Task UpdatePrivilegeAfterPurchase(string username, TicketPurchaseRequest request, Guid ticketUid, int paidByBonuses, int paidByMoney, int bonusToAdd = 0)
    {
        await _circuitBreaker.ExecuteAsync(
            "BonusService",
            async () =>
            {
                var updateRequest = new
                {
                    TicketUid = ticketUid,
                    FlightNumber = request.FlightNumber,
                    Price = request.Price,
                    PaidFromBalance = request.PaidFromBalance,
                    PaidByBonuses = paidByBonuses,
                    PaidByMoney = paidByMoney,
                    BonusToAdd = bonusToAdd
                };

                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/purchase")
                {
                    Content = content
                };
                httpRequest.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update privilege after purchase for user: {Username}. Status: {StatusCode}",
                        username, response.StatusCode);
                    throw new HttpRequestException($"Failed to update privilege: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation("Successfully updated privilege for user {Username} after purchase. Ticket: {TicketUid}, PaidByBonuses: {PaidByBonuses}, BonusToAdd: {BonusToAdd}",
                        username, ticketUid, paidByBonuses, bonusToAdd);
                }
            },
            () =>
            {
                _logger.LogWarning("Using fallback for UpdatePrivilegeAfterPurchase for user: {Username}", username);
                throw new Exception("Bonus service unavailable");
            });
    }

    public async Task UpdatePrivilegeAfterCancel(string username, Guid ticketUid)
    {
        await _circuitBreaker.ExecuteAsync(
            "BonusService",
            async () =>
            {
                var cancelRequest = new
                {
                    TicketUid = ticketUid
                };

                var json = JsonSerializer.Serialize(cancelRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/cancel")
                {
                    Content = content
                };
                httpRequest.Headers.Add("X-User-Name", username);

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update privilege after cancel for user: {Username}. Status: {StatusCode}",
                        username, response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Successfully updated privilege for user {Username} after cancel. Ticket: {TicketUid}",
                        username, ticketUid);
                }
            },
            () =>
            {
                _logger.LogWarning("Using fallback for UpdatePrivilegeAfterCancel for user: {Username}", username);
                throw new Exception("Bonus service unavailable");
            });
    }
}