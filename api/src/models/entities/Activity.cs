using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("Activity")]
public class Activity
{
    [Key]
    [Column("activity_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("strava_activity_id")]
    public long? StravaActivityId { get; set; }

    [Column("activity_type")]
    public string? ActivityType { get; set; }

    [Column("activity_name")]
    public string? ActivityName { get; set; }

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("timezone")]
    public string? Timezone { get; set; }

    [Column("distance_meters")]
    public float? DistanceMeters { get; set; }

    [Column("moving_time_seconds")]
    public int? MovingTimeSeconds { get; set; }

    [Column("elapsed_time_seconds")]
    public int? ElapsedTimeSeconds { get; set; }

    [Column("total_elevation_gain")]
    public float? TotalElevationGain { get; set; }

    [Column("avg_speed")]
    public float? AvgSpeed { get; set; }

    [Column("max_speed")]
    public float? MaxSpeed { get; set; }

    [Column("avg_heartrate")]
    public float? AvgHeartrate { get; set; }

    [Column("max_heartrate")]
    public float? MaxHeartrate { get; set; }

    [Column("avg_watts")]
    public float? AvgWatts { get; set; }

    [Column("calories")]
    public float? Calories { get; set; }

    [Column("start_latitude")]
    public float? StartLatitude { get; set; }

    [Column("start_longitude")]
    public float? StartLongitude { get; set; }

    [Column("end_latitude")]
    public float? EndLatitude { get; set; }

    [Column("end_longitude")]
    public float? EndLongitude { get; set; }

    [Column("polyline")]
    public string? Polyline { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
