using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.Services;

public interface IGatewayService
{
    Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size);

    Task<UserInfoResponse> GetUserInfoAsync(string username);

    Task<List<TicketResponse>> GetUserTicketsAsync(string username);

    Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid);

    Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request);

    Task<bool> CancelTicketAsync(string username, Guid ticketUid);

    Task<PrivilegeInfoResponse?> GetPrivilegeInfoAsync(string username);
}