using Microsoft.EntityFrameworkCore;
using TicketService.Database;
using TicketService.Dto;
using TicketService.Interfaces;
using TicketService.Models;

namespace TicketService.Services;

public class TicketService : ITicketService
{
    private readonly TicketDatabaseContext _context;

    public TicketService(TicketDatabaseContext context)
    {
        _context = context;
    }

    public async Task<Ticket> CreateTicketAsync(TicketPurchaseRequestDto request, string username)
    {
        var ticket = new Ticket
        {
            TicketUid = Guid.NewGuid(),
            Username = username,
            FlightNumber = request.FlightNumber,
            Price = request.Price,
            Status = TicketStatus.PAID
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket?> GetTicketByUidAsync(Guid ticketUid)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketUid == ticketUid);
    }

    public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(string username)
    {
        return await _context.Tickets
            .Where(t => t.Username == username)
            .ToListAsync();
    }

    public async Task<bool> UpdateTicketStatusAsync(Guid ticketUid, TicketStatus newStatus)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketUid == ticketUid);

        if (ticket == null) return false;

        ticket.Status = newStatus;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTicketAsync(Guid ticketUid)
    {
        // Вместо физического удаления помечаем как CANCELED
        return await UpdateTicketStatusAsync(ticketUid, TicketStatus.CANCELED);
    }
}