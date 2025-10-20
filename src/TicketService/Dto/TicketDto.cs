namespace TicketService.Dto;

public class TicketDto
{
    public Guid TicketUid { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string FromAirport { get; set; } = string.Empty;
    public string ToAirport { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Price { get; set; }
    public string Status { get; set; } = string.Empty;
}