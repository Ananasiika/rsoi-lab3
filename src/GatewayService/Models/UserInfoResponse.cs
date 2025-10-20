namespace GatewayService.Models;

public class UserPrivilegeInfo
{
    public int Balance { get; set; }
    public string Status { get; set; } = string.Empty;
}

// UserInfoResponse.cs - исправить
public class UserInfoResponse
{
    public List<TicketResponse> Tickets { get; set; } = new();
    public PrivilegeShortInfo Privilege { get; set; } = new();
}