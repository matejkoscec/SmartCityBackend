using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext() { }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

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
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

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
    }
}