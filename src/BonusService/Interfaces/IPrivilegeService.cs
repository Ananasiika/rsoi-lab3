using BonusService.Dto;
using BonusService.Models;

namespace BonusService.Interfaces;

public interface IPrivilegeService
{
    Task<PrivilegeDto> GetPrivilegeInfoAsync(string username);

    Task<Privilege> GetOrCreatePrivilegeAsync(string username);

    Task<PrivilegeHistory> AddPrivilegeHistoryAsync(int privilegeId, Guid ticketUid, int balanceDiff, string operationType);

    Task UpdatePrivilegeBalanceAsync(int privilegeId, int balanceDiff);

    Task<int> CalculateBonusForPurchaseAsync(int ticketPrice);

    Task ProcessPurchaseAsync(string username, PurchaseUpdateRequest request);

    Task ProcessCancelAsync(string username, CancelUpdateRequest request);
}