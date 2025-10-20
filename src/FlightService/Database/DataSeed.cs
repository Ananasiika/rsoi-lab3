using FlightService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Database;

public static class DataSeed
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new FlightDatabaseContext(
                   serviceProvider.GetRequiredService<DbContextOptions<FlightDatabaseContext>>()))
        {
            // Проверяем, есть ли уже данные
            if (context.Airports.Any() || context.Flights.Any())
            {
                return; // База уже имеет данные
            }

            // Добавляем аэропорты
            var airports = new Airport[]
            {
                new Airport
                {
                    Id = 1,
                    Name = "Шереметьево",
                    City = "Москва",
                    Country = "Россия"
                },
                new Airport
                {
                    Id = 2,
                    Name = "Пулково",
                    City = "Санкт-Петербург",
                    Country = "Россия"
                }
            };

            context.Airports.AddRange(airports);
            context.SaveChanges();

            // Добавляем рейсы
            var flights = new Flight[]
            {
                new Flight
                {
                    Id = 1,
                    FlightNumber = "AFL031",
                    DateTime = new DateTime(2021, 10, 8, 20, 0, 0, DateTimeKind.Utc),
                    FromAirportId = 2,
                    ToAirportId = 1,
                    Price = 1500
                }
            };

            context.Flights.AddRange(flights);
            context.SaveChanges();
        }
    }
}