using Microsoft.EntityFrameworkCore;
using TicketService.Models;

namespace TicketService.Database;

public class TicketDatabaseContext : DbContext
{
    public TicketDatabaseContext(DbContextOptions<TicketDatabaseContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("ticket"); // имя таблицы с маленькой буквы

            // Уникальный индекс для ticket_uid
            entity.HasIndex(t => t.TicketUid)
                .IsUnique();

            // Настройка столбцов
            entity.Property(t => t.Id)
                .HasColumnName("id");

            entity.Property(t => t.TicketUid)
                .HasColumnName("ticket_uid");

            entity.Property(t => t.Username)
                .HasColumnName("username");

            entity.Property(t => t.FlightNumber)
                .HasColumnName("flight_number");

            entity.Property(t => t.Price)
                .HasColumnName("price");

            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasColumnName("status");
        });

        base.OnModelCreating(modelBuilder);
    }
}