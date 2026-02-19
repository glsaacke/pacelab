using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("User")]
public class User
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("last_logged_in")]
    public DateTime? LastLoggedIn { get; set; }

    // TODO: Run DB migration to add these Strava columns to the User table:
    //   ALTER TABLE "User" ADD COLUMN strava_user_id BIGINT UNIQUE;
    //   ALTER TABLE "User" ADD COLUMN strava_access_token TEXT;
    //   ALTER TABLE "User" ADD COLUMN strava_refresh_token TEXT;
    //   ALTER TABLE "User" ADD COLUMN strava_token_expires_at TIMESTAMP;
    //   ALTER TABLE "User" ADD COLUMN last_strava_sync TIMESTAMP;
    [Column("strava_user_id")]
    public long? StravaUserId { get; set; }

    [Column("strava_access_token")]
    public string? StravaAccessToken { get; set; }

    [Column("strava_refresh_token")]
    public string? StravaRefreshToken { get; set; }

    [Column("strava_token_expires_at")]
    public DateTime? StravaTokenExpiresAt { get; set; }

    [Column("last_strava_sync")]
    public DateTime? LastStravaSync { get; set; }
}
