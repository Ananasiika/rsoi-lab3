using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public interface IBonusClient
{
    Task<ServiceResponse<PrivilegeInfoResponse?>> GetPrivilegeInfoAsync(string username);
    Task<ServiceResponse<PrivilegeShortInfo?>> GetPrivilegeShortInfoAsync(string username);
    Task<ServiceResponse<bool>> UpdatePrivilegeAfterPurchase(string username, TicketPurchaseRequest request, Guid ticketUid, int paidByBonuses, int paidByMoney, int bonusToAdd = 0);
    Task<ServiceResponse<bool>> UpdatePrivilegeAfterCancel(string username, Guid ticketUid);
}