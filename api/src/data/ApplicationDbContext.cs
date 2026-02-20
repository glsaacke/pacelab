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
            
            // Don't set default values - table already exists
            entity.Property(u => u.CreatedAt).ValueGeneratedNever();
            entity.Property(u => u.UpdatedAt).ValueGeneratedNever();
            entity.Property(u => u.LastLoggedIn).ValueGeneratedNever();

            // Strava fields - nullable, added via migration
            entity.Property(u => u.StravaUserId).HasColumnName("strava_user_id");
            entity.HasIndex(u => u.StravaUserId).IsUnique();
            entity.Property(u => u.StravaAccessToken).HasColumnName("strava_access_token");
            entity.Property(u => u.StravaRefreshToken).HasColumnName("strava_refresh_token");
            entity.Property(u => u.StravaTokenExpiresAt).HasColumnName("strava_token_expires_at");
            entity.Property(u => u.LastStravaSync).HasColumnName("last_strava_sync");
        });
    }
}
