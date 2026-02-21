using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("Activity_Adjustments")]
public class ActivityAdjustments
{
    [Key]
    [Column("activity_adjustments_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityAdjustmentsId { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("adjusted_speed_mps")]
    public float? AdjustedSpeedMps { get; set; }

    [Column("adjusted_time_seconds")]
    public int? AdjustedTimeSeconds { get; set; }

    [Column("wind_adjustment")]
    public float? WindAdjustment { get; set; }

    [Column("heat_adjustment")]
    public float? HeatAdjustment { get; set; }

    [Column("cold_adjustment")]
    public float? ColdAdjustment { get; set; }

    [Column("precipitation_adjustment")]
    public float? PrecipitationAdjustment { get; set; }

    [Column("elevation_adjustment")]
    public float? ElevationAdjustment { get; set; }

    [Column("elevation_gain_adjustment")]
    public float? ElevationGainAdjustment { get; set; }

    [Column("total_adjustment")]
    public float? TotalAdjustment { get; set; }

    [Column("total_adjustment_percent")]
    public float? TotalAdjustmentPercent { get; set; }

    [Column("difficulty_rating")]
    public string? DifficultyRating { get; set; }

    [Column("calculated_at")]
    public DateTime? CalculatedAt { get; set; }
}
