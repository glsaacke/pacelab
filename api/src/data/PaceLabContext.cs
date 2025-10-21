using Microsoft.EntityFrameworkCore;
using api.src.models;

namespace api.src.data
{
    public class PaceLabContext : DbContext
    {
        public PaceLabContext(DbContextOptions<PaceLabContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<StravaToken> StravaTokens { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityWeather> ActivityWeathers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd(); // Auto-increment
                entity.Property(e => e.StravaId).HasColumnName("strava_id");
                entity.Property(e => e.FirstName).HasColumnName("firstname").HasMaxLength(255);
                entity.Property(e => e.LastName).HasColumnName("lastname").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
            });

            modelBuilder.Entity<StravaToken>(entity =>
            {
                entity.ToTable("stravatoken");
                entity.HasKey(e => e.StravaTokenId);
                entity.Property(e => e.StravaTokenId)
                    .HasColumnName("stravatoken_id")
                    .ValueGeneratedOnAdd(); // Auto-increment
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AccessToken).HasColumnName("access_token").HasColumnType("longtext");
                entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasColumnType("longtext");
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            });

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.ToTable("activity");
                entity.HasKey(e => e.ActivityId);
                entity.Property(e => e.ActivityId)
                    .HasColumnName("activity_id")
                    .ValueGeneratedOnAdd(); // Auto-increment
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(255);
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.DistanceM).HasColumnName("distance_m");
                entity.Property(e => e.MovingTimeS).HasColumnName("moving_time_s");
                entity.Property(e => e.ElevationGainM).HasColumnName("elevation_gain_m");
                entity.Property(e => e.AverageSpeedMph).HasColumnName("average_speed_mph");
                entity.Property(e => e.StartLatitude).HasColumnName("start_latitude");
                entity.Property(e => e.StartLongitude).HasColumnName("start_longitude");
            });

            modelBuilder.Entity<ActivityWeather>(entity =>
            {
                entity.ToTable("activity_weather");
                entity.HasKey(e => e.ActivityWeatherId);
                entity.Property(e => e.ActivityWeatherId)
                    .HasColumnName("activity_weather_id")
                    .ValueGeneratedOnAdd(); // Auto-increment
                entity.Property(e => e.ActivityId).HasColumnName("activity_id");
                entity.Property(e => e.Temperature).HasColumnName("temperature");
                entity.Property(e => e.HumidityPct).HasColumnName("humidity_pct");
                entity.Property(e => e.WindSpeed).HasColumnName("wind_speed");
                entity.Property(e => e.Pressure).HasColumnName("pressure");
                entity.Property(e => e.FeelsLike).HasColumnName("feels_like");
                entity.Property(e => e.WeatherDescription).HasColumnName("weather_description").HasMaxLength(255);
                entity.Property(e => e.Esi).HasColumnName("esi");
                entity.Property(e => e.AdjustedPaceS).HasColumnName("adjusted_pace_s");
            });
        }
    }
}