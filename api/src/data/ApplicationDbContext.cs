using Microsoft.EntityFrameworkCore;
using api.src.models.entities;

namespace api.src.data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserStravaConnection> UserStravaConnections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity for existing table
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();

            entity.Property(u => u.CreatedAt).ValueGeneratedNever();
            entity.Property(u => u.UpdatedAt).ValueGeneratedNever();
            entity.Property(u => u.LastLoggedIn).ValueGeneratedNever();
        });

        // Configure UserStravaConnection entity for existing table
        modelBuilder.Entity<UserStravaConnection>(entity =>
        {
            entity.ToTable("User_Strava_Connection");
            entity.HasKey(c => c.UserStravaConnectionId);
            entity.HasIndex(c => c.UserId).IsUnique();
            entity.Property(c => c.StravaAccessToken).IsRequired();
            entity.Property(c => c.StravaRefreshToken).IsRequired();

            entity.Property(c => c.ConnectedAt).ValueGeneratedNever();
            entity.Property(c => c.UpdatedAt).ValueGeneratedNever();
            entity.Property(c => c.LastSync).ValueGeneratedNever();
            entity.Property(c => c.StravaTokenExpiresAt).ValueGeneratedNever();
        });
    }
}

