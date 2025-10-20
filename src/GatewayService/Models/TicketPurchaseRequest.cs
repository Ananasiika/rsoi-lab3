namespace GatewayService.Models;

public class TicketPurchaseRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool PaidFromBalance { get; set; }
}