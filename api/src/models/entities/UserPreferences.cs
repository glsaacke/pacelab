using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("User_Preferences")]
public class UserPreferences
{
    [Key]
    [Column("user_preferences_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserPreferencesId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("distance_unit")]
    public string? DistanceUnit { get; set; }

    [Column("elevation_unit")]
    public string? ElevationUnit { get; set; }

    [Column("temperature_unit")]
    public string? TemperatureUnit { get; set; }

    [Column("speed_format")]
    public string? SpeedFormat { get; set; }

    [Column("pace_format")]
    public string? PaceFormat { get; set; }

    [Column("wind_speed_unit")]
    public string? WindSpeedUnit { get; set; }

    [Column("sync_cycling")]
    public bool? SyncCycling { get; set; }

    [Column("sync_running")]
    public bool? SyncRunning { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
