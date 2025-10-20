namespace FlightService.Models;

public class FlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public Airport FromAirport { get; set; }
    public Airport ToAirport { get; set; }
    public DateTime Date { get; set; }
    public int Price { get; set; }
}