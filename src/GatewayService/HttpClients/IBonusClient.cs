using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface IBonusClient
{
    Task<PrivilegeInfoResponse?> GetPrivilegeInfoAsync(string username);

    Task<PrivilegeShortInfo?> GetPrivilegeShortInfoAsync(string username);

    Task UpdatePrivilegeAfterPurchase(string username, TicketPurchaseRequest request, Guid ticketUid, int paidByBonuses, int paidByMoney, int bonusToAdd = 0);

    Task UpdatePrivilegeAfterCancel(string username, Guid ticketUid);
}