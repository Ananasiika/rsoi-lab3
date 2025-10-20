namespace GatewayService.Models;

public class FlightResponse
{
    public string FlightNumber { get; set; } = string.Empty;
    public string FromAirport { get; set; }
    public string ToAirport { get; set; }
    public DateTime Date { get; set; }
    public int Price { get; set; }
}