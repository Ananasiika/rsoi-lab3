using GatewayService.Dto;
using GatewayService.HttpClients;
using GatewayService.Models;

namespace GatewayService.Services;

public class GatewayService : IGatewayService
{
    private readonly IFlightClient _flightClient;
    private readonly IBonusClient _bonusClient;
    private readonly ITicketClient _ticketClient;
    private readonly ILogger<GatewayService> _logger;
    private readonly IRetryQueue _retryQueue;

    public GatewayService(
        IFlightClient flightClient,
        IBonusClient bonusClient,
        ITicketClient ticketClient,
        IRetryQueue retryQueue,
        ILogger<GatewayService> logger)
    {
        _flightClient = flightClient;
        _bonusClient = bonusClient;
        _retryQueue = retryQueue;
        _ticketClient = ticketClient;
        _logger = logger;
    }

    public Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size)
    {
        return _flightClient.GetFlightsAsync(page, size);
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string username)
    {
        var tickets = await _ticketClient.GetUserTicketsAsync(username);
        var result = new List<TicketResponse>();

        foreach (var ticket in tickets)
        {
            // 2. Для каждого билета получаем информацию о рейсе из FlightService
            var flight = await _flightClient.GetFlightByNumberAsync(ticket.FlightNumber);

            if (flight != null)
            {
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = $"{flight.FromAirport.City} {flight.FromAirport.Name}",
                    ToAirport = $"{flight.ToAirport.City} {flight.ToAirport.Name}",
                    Date = flight.Date,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
            else
            {
                // Если информация о рейсе не найдена, возвращаем базовую информацию
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown",
                    Date = DateTime.MinValue,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
        }
        var privilege = await _bonusClient.GetPrivilegeShortInfoAsync(username);

        return new UserInfoResponse
        {
            Tickets = result,
            Privilege = new PrivilegeShortInfo
            {
                Balance = privilege?.Balance ?? 0,
                Status = privilege?.Status ?? "BRONZE"
            }
        };
    }

    public async Task<List<TicketResponse>> GetUserTicketsAsync(string username)
    {
        // 1. Получаем билеты из TicketService
        var tickets = await _ticketClient.GetUserTicketsAsync(username);

        var result = new List<TicketResponse>();

        foreach (var ticket in tickets)
        {
            // 2. Для каждого билета получаем информацию о рейсе из FlightService
            var flight = await _flightClient.GetFlightByNumberAsync(ticket.FlightNumber);

            if (flight != null)
            {
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = $"{flight.FromAirport.City} {flight.FromAirport.Name}",
                    ToAirport = $"{flight.ToAirport.City} {flight.ToAirport.Name}",
                    Date = flight.Date,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
            else
            {
                // Если информация о рейсе не найдена, возвращаем базовую информацию
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown",
                    Date = DateTime.MinValue,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
        }

        return result;
    }

    public async Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid)
    {
        // 1. Получаем билет из TicketService
        var ticket = await _ticketClient.GetTicketAsync(username, ticketUid);
        if (ticket == null) return null;

        // 2. Получаем информацию о рейсе из FlightService
        var flight = await _flightClient.GetFlightByNumberAsync(ticket.FlightNumber);

        if (flight == null) return null;

        return new TicketResponse
        {
            TicketUid = ticket.TicketUid,
            FlightNumber = ticket.FlightNumber,
            FromAirport = $"{flight.FromAirport.City} {flight.FromAirport.Name}",
            ToAirport = $"{flight.ToAirport.City} {flight.ToAirport.Name}",
            Date = flight.Date,
            Price = ticket.Price,
            Status = ticket.Status
        };
    }

    public async Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request)
    {
        try
        {
            _logger.LogInformation("Starting ticket purchase for user: {Username}, flight: {FlightNumber}",
                username, request.FlightNumber);

            // 1. Получить информацию о полете
            var flight = await _flightClient.GetFlightByNumberAsync(request.FlightNumber);
            if (flight == null)
            {
                _logger.LogWarning("Flight not found: {FlightNumber}", request.FlightNumber);
                return null;
            }

            // 2. Получить информацию о бонусах
            var privilegeInfo = await _bonusClient.GetPrivilegeShortInfoAsync(username);

            // 3. Рассчитать суммы оплаты и бонусы
            int paidByMoney, paidByBonuses, bonusToAdd;
            CalculatePaidAmounts(request, privilegeInfo, out paidByBonuses, out paidByMoney, out bonusToAdd);

            // 4. Создать запрос на покупку билета в TicketService
            var ticketPurchaseRequest = new TicketPurchaseRequest
            {
                FlightNumber = request.FlightNumber,
                Price = request.Price,
                PaidFromBalance = request.PaidFromBalance
            };

            var ticketResponse = await _ticketClient.PurchaseTicketAsync(username, ticketPurchaseRequest);
            if (ticketResponse == null)
            {
                _logger.LogWarning("Failed to create ticket in TicketService");
                return null;
            }

            // 5. Обновить бонусную систему
            try
            {
                await _bonusClient.UpdatePrivilegeAfterPurchase(
                    username, request, ticketResponse.TicketUid,
                    paidByBonuses, paidByMoney, bonusToAdd);
            }
            catch (Exception bonusEx)
            {
                _logger.LogError(bonusEx, "Failed to update bonus system, rolling back ticket creation");

                // Rollback ticket creation
                await _ticketClient.CancelTicketAsync(username, ticketResponse.TicketUid);

                throw new Exception("Purchase failed: bonus system unavailable", bonusEx);
            }

            // 6. Получить обновленную информацию о бонусах
            var updatedPrivilege = await _bonusClient.GetPrivilegeShortInfoAsync(username);

            // 7. Вернуть ответ в ожидаемом формате
            return new TicketPurchaseResponse
            {
                TicketUid = ticketResponse.TicketUid,
                FlightNumber = flight.FlightNumber,
                FromAirport = flight.FromAirport.City + " " + flight.FromAirport.Name,
                ToAirport = flight.ToAirport.City + " " + flight.ToAirport.Name,
                Date = flight.Date,
                Price = request.Price,
                PaidByMoney = paidByMoney,
                PaidByBonuses = paidByBonuses,
                Status = "PAID",
                Privilege = updatedPrivilege ?? new PrivilegeShortInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing ticket for user: {Username}", username);
            return null;
        }
    }

    public async Task<bool> CancelTicketAsync(string username, Guid ticketUid)
    {
        // 1. Отменяем билет
        var success = await _ticketClient.CancelTicketAsync(username, ticketUid);
        if (!success)
        {
            return false;
        }

        // 2. Обновляем бонусный счет
        try
        {
            // 2. Update bonus system - if fails, add to retry queue
            await _bonusClient.UpdatePrivilegeAfterCancel(username, ticketUid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bonus service unavailable for cancel operation, adding to retry queue");

            // Add to retry queue - user gets success response anyway
            _retryQueue.Enqueue(new RetryItem
            {
                OperationType = "UpdatePrivilegeAfterCancel",
                Username = username,
                Data = new CancelData { TicketUid = ticketUid }
            });
        }

        return true;
    }

    public Task<PrivilegeInfoResponse?> GetPrivilegeInfoAsync(string username)
    {
        return _bonusClient.GetPrivilegeInfoAsync(username);
    }

    private int CalculateBonusToAdd(int price, string status)
    {
        // 10% от стоимости билета
        return (int)(price * 0.1);
    }

    private int CalculatePaidAmounts(TicketPurchaseRequest request, PrivilegeShortInfo? privilege, out int paidByBonuses, out int paidByMoney, out int bonusToAdd)
    {
        paidByBonuses = 0;
        paidByMoney = request.Price;
        bonusToAdd = 0;

        if (request.PaidFromBalance && privilege != null)
        {
            // Логика оплаты бонусами
            paidByBonuses = Math.Min(privilege.Balance, request.Price);
            paidByMoney = request.Price - paidByBonuses;
        }

        // Начисление бонусов (10% от стоимости)
        bonusToAdd = CalculateBonusToAdd(request.Price, privilege?.Status ?? "BRONZE");

        return paidByMoney;
    }
}