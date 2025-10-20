using BonusService.Database;
using BonusService.Dto;
using BonusService.Interfaces;
using BonusService.Models;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Services;

public class PrivilegeService : IPrivilegeService
{
    private readonly BonusDatabaseContext _context;

    public PrivilegeService(BonusDatabaseContext context)
    {
        _context = context;
    }

    public async Task<PrivilegeDto> GetPrivilegeInfoAsync(string username)
    {
        var privilege = await GetOrCreatePrivilegeAsync(username);

        var history = await _context.PrivilegeHistories
            .Where(ph => ph.PrivilegeId == privilege.Id)
            .OrderByDescending(ph => ph.Datetime)
            .Select(ph => new PrivilegeHistoryDto
            {
                Date = ph.Datetime,
                TicketUid = ph.TicketUid,
                BalanceDiff = ph.BalanceDiff,
                OperationType = ph.OperationType
            })
            .ToListAsync();

        return new PrivilegeDto
        {
            Balance = privilege.Balance,
            Status = privilege.Status,
            History = history
        };
    }

    public async Task<Privilege> GetOrCreatePrivilegeAsync(string username)
    {
        var privilege = await _context.Privileges
            .Include(p => p.History)
            .FirstOrDefaultAsync(p => p.Username == username);

        if (privilege == null)
        {
            privilege = new Privilege
            {
                Username = username,
                Status = PrivilegeStatus.BRONZE,
                Balance = 0
            };

            _context.Privileges.Add(privilege);
            await _context.SaveChangesAsync();
        }

        return privilege;
    }

    public async Task<PrivilegeHistory> AddPrivilegeHistoryAsync(int privilegeId, Guid ticketUid,
        int balanceDiff, string operationType)
    {
        var history = new PrivilegeHistory
        {
            PrivilegeId = privilegeId,
            TicketUid = ticketUid,
            Datetime = DateTime.UtcNow,
            BalanceDiff = balanceDiff,
            OperationType = operationType
        };

        _context.PrivilegeHistories.Add(history);
        await _context.SaveChangesAsync();

        return history;
    }

    public Task<int> CalculateBonusForPurchaseAsync(int ticketPrice)
    {
        // 10% от стоимости билета
        return Task.FromResult((int)(ticketPrice * 0.1));
    }

    public async Task ProcessPurchaseAsync(string username, PurchaseUpdateRequest request)
    {
        var privilege = await GetOrCreatePrivilegeAsync(username);

        if (request.PaidFromBalance)
        {
            // Списание бонусов при оплате
            if (request.PaidByBonuses > 0)
            {
                await UpdatePrivilegeBalanceAsync(privilege.Id, -request.PaidByBonuses);
                await AddPrivilegeHistoryAsync(privilege.Id, request.TicketUid, -request.PaidByBonuses, OperationType.DEBIT_THE_ACCOUNT);
            }
        }
        else
        {
            // Начисление бонусов при обычной оплате
            if (request.BonusToAdd > 0)
            {
                await UpdatePrivilegeBalanceAsync(privilege.Id, request.BonusToAdd);
                await AddPrivilegeHistoryAsync(privilege.Id, request.TicketUid, request.BonusToAdd, OperationType.FILL_IN_BALANCE);
            }
        }

        // Обновляем статус привилегии после изменения баланса
        UpdatePrivilegeStatus(privilege);
        await _context.SaveChangesAsync();
    }

    public async Task ProcessCancelAsync(string username, CancelUpdateRequest request)
    {
        var privilege = await GetOrCreatePrivilegeAsync(username);

        // Ищем историю операций по этому билету
        var purchaseHistory = await _context.PrivilegeHistories
            .Where(ph => ph.PrivilegeId == privilege.Id && ph.TicketUid == request.TicketUid)
            .OrderByDescending(ph => ph.Datetime)
            .FirstOrDefaultAsync();

        if (purchaseHistory != null)
        {
            // Отменяем предыдущую операцию
            if (purchaseHistory.OperationType == OperationType.FILL_IN_BALANCE)
            {
                // Списание начисленных бонусов
                await UpdatePrivilegeBalanceAsync(privilege.Id, -purchaseHistory.BalanceDiff);
                await AddPrivilegeHistoryAsync(privilege.Id, request.TicketUid, -purchaseHistory.BalanceDiff, OperationType.DEBIT_THE_ACCOUNT);
            }
            else if (purchaseHistory.OperationType == OperationType.DEBIT_THE_ACCOUNT)
            {
                // Возврат списанных бонусов
                await UpdatePrivilegeBalanceAsync(privilege.Id, Math.Abs(purchaseHistory.BalanceDiff));
                await AddPrivilegeHistoryAsync(privilege.Id, request.TicketUid, Math.Abs(purchaseHistory.BalanceDiff), OperationType.FILL_IN_BALANCE);
            }
        }

        // Обновляем статус привилегии после изменения баланса
        UpdatePrivilegeStatus(privilege);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePrivilegeBalanceAsync(int privilegeId, int balanceDiff)
    {
        var privilege = await _context.Privileges.FindAsync(privilegeId);
        if (privilege != null)
        {
            privilege.Balance += balanceDiff;
            // Гарантируем, что баланс не станет отрицательным
            if (privilege.Balance < 0)
                privilege.Balance = 0;

            // Обновляем статус при изменении баланса
            UpdatePrivilegeStatus(privilege);

            await _context.SaveChangesAsync();
        }
    }

    // Сделайте метод UpdatePrivilegeStatus публичным или оставьте приватным, но вызывайте его везде
    private void UpdatePrivilegeStatus(Privilege privilege)
    {
        // Логика обновления статуса в зависимости от баланса
        if (privilege.Balance >= 10000)
            privilege.Status = PrivilegeStatus.GOLD;
        else if (privilege.Balance >= 5000)
            privilege.Status = PrivilegeStatus.SILVER;
        else
            privilege.Status = PrivilegeStatus.BRONZE;
    }
}