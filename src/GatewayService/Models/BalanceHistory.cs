namespace GatewayService.Models;

public class BalanceHistory
{
    public DateTime Date { get; set; }
    public Guid TicketUid { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;
}