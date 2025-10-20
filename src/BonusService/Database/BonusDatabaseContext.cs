using BonusService.Models;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Database;

public class BonusDatabaseContext : DbContext
{
    public BonusDatabaseContext(DbContextOptions<BonusDatabaseContext> options) : base(options)
    {
    }

    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<PrivilegeHistory> PrivilegeHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Privilege>(entity =>
        {
            entity.ToTable("privilege");

            entity.HasIndex(p => p.Username)
                .IsUnique();

            entity.Property(p => p.Id)
                .HasColumnName("id"); // явно указываем имя столбца

            entity.Property(p => p.Username)
                .HasColumnName("username");

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasDefaultValue("BRONZE")
                .HasColumnName("status");

            entity.Property(p => p.Balance)
                .HasColumnName("balance");
        });

        modelBuilder.Entity<PrivilegeHistory>(entity =>
        {
            entity.ToTable("privilege_history"); // имя таблицы с маленькой буквы

            entity.Property(ph => ph.Id)
                .HasColumnName("id");

            entity.Property(ph => ph.PrivilegeId)
                .HasColumnName("privilege_id");

            entity.Property(ph => ph.TicketUid)
                .HasColumnName("ticket_uid");

            entity.Property(ph => ph.Datetime)
                .HasColumnName("datetime");

            entity.Property(ph => ph.BalanceDiff)
                .HasColumnName("balance_diff");

            entity.Property(ph => ph.OperationType)
                .HasConversion<string>()
                .HasColumnName("operation_type");

            // Связь между Privilege и PrivilegeHistory
            entity.HasOne(ph => ph.Privilege)
                .WithMany(p => p.History)
                .HasForeignKey(ph => ph.PrivilegeId);
        });

        base.OnModelCreating(modelBuilder);
    }
}