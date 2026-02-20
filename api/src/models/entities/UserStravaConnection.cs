using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("User_Strava_Connection")]
public class UserStravaConnection
{
    [Key]
    [Column("user_strava_connection_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserStravaConnectionId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("strava_user_id")]
    public long? StravaUserId { get; set; }

    [Column("strava_username")]
    public string? StravaUsername { get; set; }

    [Required]
    [Column("strava_access_token")]
    public string StravaAccessToken { get; set; } = string.Empty;

    [Required]
    [Column("strava_refresh_token")]
    public string StravaRefreshToken { get; set; } = string.Empty;

    [Column("strava_token_expires_at")]
    public DateTime? StravaTokenExpiresAt { get; set; }

    [Column("last_sync")]
    public DateTime? LastSync { get; set; }

    [Column("connected_at")]
    public DateTime? ConnectedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
