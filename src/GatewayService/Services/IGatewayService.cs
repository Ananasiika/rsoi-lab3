using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.Services;

public interface IGatewayService
{
    Task<ServiceResponse<PaginationResponse<FlightDto>>> GetFlightsAsync(int page, int size);
    Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync(string username);
    Task<ServiceResponse<List<TicketResponse>>> GetUserTicketsAsync(string username);
    Task<ServiceResponse<TicketResponse?>> GetTicketAsync(string username, Guid ticketUid);
    Task<ServiceResponse<TicketPurchaseResponse?>> PurchaseTicketAsync(string username, TicketPurchaseRequest request);
    Task<ServiceResponse<bool>> CancelTicketAsync(string username, Guid ticketUid);
    Task<ServiceResponse<PrivilegeInfoResponse?>> GetPrivilegeInfoAsync(string username);
}