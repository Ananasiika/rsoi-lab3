using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface ITicketClient
{
    Task<List<TicketResponse>> GetUserTicketsAsync(string username);

    Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid);

    Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request);

    Task<bool> CancelTicketAsync(string username, Guid ticketUid);
}