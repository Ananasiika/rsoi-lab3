namespace BonusService.Dto;

public class PrivilegeDto
{
    public int Balance { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PrivilegeHistoryDto> History { get; set; } = new();
}

public class PrivilegeHistoryDto
{
    public DateTime Date { get; set; }
    public Guid TicketUid { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;
}