using Gomotel.Domain.Entities;
using Gomotel.Domain.ValueObjects;
using Gomotel.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gomotel.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Motel> Motels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Motel entity
        builder.Entity<Motel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            // Configure Address value object
            entity.OwnsOne(
                e => e.Address,
                address =>
                {
                    address.Property(a => a.Street).IsRequired().HasMaxLength(200);
                    address.Property(a => a.City).IsRequired().HasMaxLength(100);
                    address.Property(a => a.State).IsRequired().HasMaxLength(100);
                    address.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);
                    address.Property(a => a.Country).IsRequired().HasMaxLength(100);
                }
            );

            // Configure relationships
            entity
                .HasMany(e => e.Rooms)
                .WithOne(r => r.Motel)
                .HasForeignKey(r => r.MotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Room entity
        builder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            // Configure Money value object
            entity.OwnsOne(
                e => e.PricePerHour,
                money =>
                {
                    money.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency).HasMaxLength(3);
                }
            );

            // Configure relationships
            entity
                .HasMany(e => e.Reservations)
                .WithOne(r => r.Room)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Reservation entity
        builder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SpecialRequests).HasMaxLength(1000);

            // Configure TimeRange value object
            entity.OwnsOne(
                e => e.TimeRange,
                timeRange =>
                {
                    timeRange.Property(t => t.StartTime).IsRequired();
                    timeRange.Property(t => t.EndTime).IsRequired();
                }
            );

            // Configure Money value object
            entity.OwnsOne(
                e => e.TotalAmount,
                money =>
                {
                    money.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency).HasMaxLength(3);
                }
            );

            // Configure relationships
            entity
                .HasOne(e => e.Motel)
                .WithMany()
                .HasForeignKey(e => e.MotelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(e => e.Payment)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Payment>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Payment entity
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(500);

            // Configure Money value object
            entity.OwnsOne(
                e => e.Amount,
                money =>
                {
                    money.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency).HasMaxLength(3);
                }
            );
        });

        // Configure indexes
        builder.Entity<Motel>().HasIndex(e => e.OwnerId);

        builder.Entity<Room>().HasIndex(e => new { e.MotelId, e.RoomNumber }).IsUnique();

        builder.Entity<Reservation>().HasIndex(e => e.UserId);

        builder.Entity<Reservation>().HasIndex(e => new { e.RoomId, e.Status });
    }
}
