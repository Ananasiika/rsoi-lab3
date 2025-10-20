namespace TicketService.Dto;

public class TicketPurchaseRequestDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool PaidFromBalance { get; set; }
}