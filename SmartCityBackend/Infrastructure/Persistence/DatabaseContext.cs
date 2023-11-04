using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext() { }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<ActiveReservation> ActiveReservations => Set<ActiveReservation>();

    public DbSet<ReservationHistory> ReservationHistory => Set<ReservationHistory>();

    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();

    public DbSet<ParkingSpotHistory> ParkingSpotsHistory => Set<ParkingSpotHistory>();

    public DbSet<ZonePrice> ZonePrices => Set<ZonePrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.PreferredUsername).HasColumnName("preferred_username").IsRequired();
            entity.Property(e => e.GivenName).HasColumnName("given_name").IsRequired();
            entity.Property(e => e.FamilyName).HasColumnName("family_name").IsRequired();
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified").IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasMany(e => e.Roles)
                .WithMany(e => e.Users)
                .UsingEntity(
                    "user_role",
                    r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("FK_user_role_role_id"),
                    l => l.HasOne(typeof(User)).WithMany().HasForeignKey("FK_user_role_user_id")
                );

            entity.HasMany(e => e.ActiveReservations)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.Name).HasColumnName("name").IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasMany(e => e.Users)
                .WithMany(e => e.Roles)
                .UsingEntity(
                    "user_role",
                    r => r.HasOne(typeof(Role)).WithMany().HasForeignKey("FK_user_role_role_id"),
                    l => l.HasOne(typeof(User)).WithMany().HasForeignKey("FK_user_role_user_id")
                );
        });

        modelBuilder.Entity<ActiveReservation>(entity =>
        {
            entity.ToTable("active_reservation");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(e => e.ActiveReservations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(e => e.Start).HasColumnName("start").HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.End).HasColumnName("end").HasColumnType("timestamp with time zone").IsRequired();
        });

        modelBuilder.Entity<ReservationHistory>(entity =>
        {
            entity.ToTable("reservation_history");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(e => e.ReservationsHistory)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(e => e.Start).HasColumnName("start").HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(e => e.End).HasColumnName("end").HasColumnType("timestamp with time zone").IsRequired();
        });

        modelBuilder.Entity<ParkingSpot>(entity =>
        {
            entity.ToTable("parking_spot");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.Lat).HasColumnName("lat").IsRequired();
            entity.Property(e => e.Lng).HasColumnName("lng").IsRequired();
            entity.Property(e => e.Zone).HasColumnName("zone").IsRequired();
        });

        modelBuilder.Entity<ParkingSpotHistory>(entity =>
        {
            entity.ToTable("parking_spot_history");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.IsOccupied).HasColumnName("is_occupied").IsRequired();
            entity.Property(e => e.StartTime)
                .HasColumnName("start_time")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(e => e.ParkingSpot)
                .WithMany(e => e.ParkingSpotsHistory)
                .HasForeignKey(e => e.ParkingSpotId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ActiveReservation)
                .WithMany(e => e.ParkingSpotsHistory)
                .HasForeignKey(e => e.ActiveReservationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ReservationHistory)
                .WithMany(e => e.ParkingSpotsHistory)
                .HasForeignKey(e => e.ReservationHistoryId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ZonePrice)
                .WithMany(e => e.ParkingSpotsHistory)
                .HasForeignKey(e => e.ZonePriceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ZonePrice>(entity =>
        {
            entity.ToTable("zone_price");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(e => e.Zone).HasColumnName("zone").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").IsRequired();

            entity.HasMany(e => e.ParkingSpotsHistory)
                .WithOne(e => e.ZonePrice)
                .HasForeignKey(e => e.ZonePriceId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}