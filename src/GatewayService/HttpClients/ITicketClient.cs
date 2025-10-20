using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface ITicketClient
{
    Task<ServiceResponse<List<TicketResponse>>> GetUserTicketsAsync(string username);
    Task<ServiceResponse<TicketResponse?>> GetTicketAsync(string username, Guid ticketUid);
    Task<ServiceResponse<TicketPurchaseResponse?>> PurchaseTicketAsync(string username, TicketPurchaseRequest request);
    Task<ServiceResponse<bool>> CancelTicketAsync(string username, Guid ticketUid);
}