using TicketService.Dto;
using TicketService.Models;

namespace TicketService.Interfaces;

public interface ITicketService
{
    Task<Ticket> CreateTicketAsync(TicketPurchaseRequestDto request, string username);

    Task<Ticket?> GetTicketByUidAsync(Guid ticketUid);

    Task<IEnumerable<Ticket>> GetUserTicketsAsync(string username);

    Task<bool> UpdateTicketStatusAsync(Guid ticketUid, TicketStatus newStatus);

    Task<bool> DeleteTicketAsync(Guid ticketUid);
}