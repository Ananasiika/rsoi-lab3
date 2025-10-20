namespace BonusService.Dto;

public class PurchaseUpdateRequest
{
    public Guid TicketUid { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool PaidFromBalance { get; set; }
    public int PaidByBonuses { get; set; }
    public int PaidByMoney { get; set; }
    public int BonusToAdd { get; set; }
}