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
        }
    }
}