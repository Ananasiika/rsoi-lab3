namespace GatewayService.Dto;

public class FlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public AirportDto FromAirport { get; set; }
    public AirportDto ToAirport { get; set; }
    public DateTime Date { get; set; }
    public int Price { get; set; }
}