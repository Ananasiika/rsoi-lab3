using FlightService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Database;

public class FlightDatabaseContext : DbContext
{
    public FlightDatabaseContext(DbContextOptions<FlightDatabaseContext> options) : base(options)
    {
    }

    public DbSet<Flight> Flights { get; set; }
    public DbSet<Airport> Airports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Таблица airport
        modelBuilder.Entity<Airport>(entity =>
        {
            entity.ToTable("airport");

            entity.Property(a => a.Id)
                .HasColumnName("id");

            entity.Property(a => a.Name)
                .HasColumnName("name");

            entity.Property(a => a.City)
                .HasColumnName("city");

            entity.Property(a => a.Country)
                .HasColumnName("country");
        });

        // Таблица flight
        modelBuilder.Entity<Flight>(entity =>
        {
            entity.ToTable("flight");

            entity.Property(f => f.Id)
                .HasColumnName("id");

            entity.Property(f => f.FlightNumber)
                .HasColumnName("flight_number");

            entity.Property(f => f.DateTime)
                .HasColumnName("datetime");

            entity.Property(f => f.FromAirportId)
                .HasColumnName("from_airport_id");

            entity.Property(f => f.ToAirportId)
                .HasColumnName("to_airport_id");

            entity.Property(f => f.Price)
                .HasColumnName("price");

            // Связи
            entity.HasOne(f => f.FromAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.FromAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.ToAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ToAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Уникальный индекс для номера рейса
            entity.HasIndex(f => f.FlightNumber)
                .IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}