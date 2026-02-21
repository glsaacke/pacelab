using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.src.models.entities;

[Table("Activity_Weather")]
public class ActivityWeather
{
    [Key]
    [Column("activity_weather_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityWeatherId { get; set; }

    [Column("activity_id")]
    public int ActivityId { get; set; }

    [Column("temperature_celsius")]
    public float? TemperatureCelsius { get; set; }

    [Column("wind_speed_mps")]
    public float? WindSpeedMps { get; set; }

    [Column("wind_direction_degrees")]
    public int? WindDirectionDegrees { get; set; }

    [Column("humidity_percent")]
    public int? HumidityPercent { get; set; }

    [Column("precipitation_mm")]
    public float? PrecipitationMm { get; set; }

    [Column("weather_condition")]
    public string? WeatherCondition { get; set; }

    [Column("feels_like_celsius")]
    public float? FeelsLikeCelsius { get; set; }

    [Column("fetched_at")]
    public DateTime? FetchedAt { get; set; }
}
