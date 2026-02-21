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
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityWeather> ActivityWeathers { get; set; }
    public DbSet<ActivityAdjustments> ActivityAdjustments { get; set; }
    public DbSet<SyncLog> SyncLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
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

        // UserStravaConnection
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

        // UserPreferences
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.ToTable("User_Preferences");
            entity.HasKey(p => p.UserPreferencesId);
            entity.HasIndex(p => p.UserId).IsUnique();
            entity.Property(p => p.CreatedAt).ValueGeneratedNever();
            entity.Property(p => p.UpdatedAt).ValueGeneratedNever();
        });

        // Activity
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activity");
            entity.HasKey(a => a.ActivityId);
            entity.HasIndex(a => a.StravaActivityId).IsUnique();
            entity.Property(a => a.CreatedAt).ValueGeneratedNever();
            entity.Property(a => a.UpdatedAt).ValueGeneratedNever();
        });

        // ActivityWeather
        modelBuilder.Entity<ActivityWeather>(entity =>
        {
            entity.ToTable("Activity_Weather");
            entity.HasKey(w => w.ActivityWeatherId);
            entity.HasIndex(w => w.ActivityId).IsUnique();
            entity.Property(w => w.FetchedAt).ValueGeneratedNever();
        });

        // ActivityAdjustments
        modelBuilder.Entity<ActivityAdjustments>(entity =>
        {
            entity.ToTable("Activity_Adjustments");
            entity.HasKey(adj => adj.ActivityAdjustmentsId);
            entity.HasIndex(adj => adj.ActivityId).IsUnique();
            entity.Property(adj => adj.CalculatedAt).ValueGeneratedNever();
        });

        // SyncLog
        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.ToTable("Sync_Logs");
            entity.HasKey(s => s.SyncLogId);
            entity.Property(s => s.StartedAt).ValueGeneratedNever();
            entity.Property(s => s.CompletedAt).ValueGeneratedNever();
        });
    }
}

