using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketService.Controllers;
using TicketService.Database;
using TicketService.Dto;
using TicketService.Models;

namespace Tests;

public class TicketServiceTests : IDisposable
{
    private readonly TicketDatabaseContext _context;
    private readonly TicketsController _ticketsController;

    public TicketServiceTests()
    {
        _context = DbHelper.CreateContext<TicketDatabaseContext>();
        var ticketService = new TicketService.Services.TicketService(_context);
        _ticketsController = new TicketsController(ticketService);

        SeedData();
    }

    private void SeedData()
    {
        // Clear existing data first
        _context.Tickets.RemoveRange(_context.Tickets);
        _context.SaveChanges();

        var tickets = new List<Ticket>
        {
            new Ticket
            {
                Id = 1,
                TicketUid = Guid.NewGuid(),
                FlightNumber = "FL123",
                Price = 5000,
                Status = TicketStatus.PAID,
                Username = "user1"
            },
            new() {
                Id = 2,
                TicketUid = Guid.NewGuid(),
                FlightNumber = "FL456",
                Price = 4500,
                Status = TicketStatus.PAID,
                Username = "user1"
            },
            new() {
                Id = 3,
                TicketUid = Guid.NewGuid(),
                FlightNumber = "FL789",
                Price = 6000,
                Status = TicketStatus.PAID,
                Username = "user2"
            }
        };

        _context.Tickets.AddRange(tickets);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetUserTickets_MissingUsername_ReturnsBadRequest()
    {
        // Act
        var result = await _ticketsController.GetUserTickets("");

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, actionResult.StatusCode);
    }

    [Fact]
    public async Task GetTicket_ValidUserAndTicket_ReturnsOk()
    {
        // Arrange
        var existingTicket = await _context.Tickets.FirstAsync();
        var ticketUid = existingTicket.TicketUid;
        var username = existingTicket.Username;

        // Act
        var result = await _ticketsController.GetTicket(ticketUid, username);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var ticket = actionResult.Value;

        Assert.NotNull(ticket);

        // Check ticket properties using reflection
        var flightNumberProperty = ticket.GetType().GetProperty("FlightNumber");
        var priceProperty = ticket.GetType().GetProperty("Price");

        Assert.Equal(existingTicket.FlightNumber, flightNumberProperty?.GetValue(ticket)?.ToString());
        Assert.Equal(existingTicket.Price, (int?)priceProperty?.GetValue(ticket));
    }

    [Fact]
    public async Task GetTicket_InvalidUser_ReturnsNotFound()
    {
        // Arrange
        var existingTicket = await _context.Tickets.FirstAsync();
        var ticketUid = existingTicket.TicketUid;

        // Act
        var result = await _ticketsController.GetTicket(ticketUid, "wronguser");

        // Assert
        var actionResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PurchaseTicket_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new TicketPurchaseRequestDto
        {
            FlightNumber = "FL999",
            Price = 7000
        };

        var username = "user3";

        // Act
        var result = await _ticketsController.PurchaseTicket(request, username);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var ticket = actionResult.Value;

        Assert.NotNull(ticket);

        // Check ticket properties using reflection
        var flightNumberProperty = ticket.GetType().GetProperty("FlightNumber");
        var priceProperty = ticket.GetType().GetProperty("Price");
        var statusProperty = ticket.GetType().GetProperty("Status");

        Assert.Equal("FL999", flightNumberProperty?.GetValue(ticket)?.ToString());
        Assert.Equal(7000, (int?)priceProperty?.GetValue(ticket));
        Assert.Equal("PAID", statusProperty?.GetValue(ticket)?.ToString());

        // Verify ticket was created
        var createdTicket = await _context.Tickets.FirstOrDefaultAsync(t =>
            t.Username == username && t.FlightNumber == "FL999");
        Assert.NotNull(createdTicket);
        Assert.Equal(7000, createdTicket.Price);
        Assert.Equal(TicketStatus.PAID, createdTicket.Status);
    }

    [Fact]
    public async Task DeleteTicket_ValidUserAndTicket_ReturnsNoContent()
    {
        // Arrange
        var existingTicket = await _context.Tickets.FirstAsync();
        var ticketUid = existingTicket.TicketUid;
        var username = existingTicket.Username;

        // Act
        var result = await _ticketsController.DeleteTicket(ticketUid, username);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify ticket status was updated to CANCELED
        var canceledTicket = await _context.Tickets.FirstAsync(t => t.TicketUid == ticketUid);
        Assert.Equal(TicketStatus.CANCELED, canceledTicket.Status);
    }

    [Fact]
    public async Task DeleteTicket_InvalidUser_ReturnsNotFound()
    {
        // Arrange
        var existingTicket = await _context.Tickets.FirstAsync();
        var ticketUid = existingTicket.TicketUid;

        // Act
        var result = await _ticketsController.DeleteTicket(ticketUid, "wronguser");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}