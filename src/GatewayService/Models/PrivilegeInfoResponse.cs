namespace GatewayService.Models;

public class PrivilegeInfoResponse
{
    public int Balance { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BalanceHistory> History { get; set; } = new();
}