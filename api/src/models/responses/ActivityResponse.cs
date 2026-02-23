namespace api.src.models.responses;

public class ActivityResponse
{
    public int ActivityId { get; set; }
    public int UserId { get; set; }
    public long? StravaActivityId { get; set; }
    public string? ActivityType { get; set; }
    public string? ActivityName { get; set; }
    public DateTime? StartDate { get; set; }
    public string? Timezone { get; set; }
    public float? DistanceMeters { get; set; }
    public int? MovingTimeSeconds { get; set; }
    public int? ElapsedTimeSeconds { get; set; }
    public float? TotalElevationGain { get; set; }
    public float? AvgSpeed { get; set; }
    public float? MaxSpeed { get; set; }
    public float? AvgHeartrate { get; set; }
    public float? MaxHeartrate { get; set; }
    public float? AvgWatts { get; set; }
    public float? Calories { get; set; }
    public float? StartLatitude { get; set; }
    public float? StartLongitude { get; set; }
    public float? EndLatitude { get; set; }
    public float? EndLongitude { get; set; }
    public string? Polyline { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties for frontend convenience
    public float? DistanceMiles => DistanceMeters.HasValue ? DistanceMeters.Value / 1609.34f : null;
    public float? DistanceKilometers => DistanceMeters.HasValue ? DistanceMeters.Value / 1000f : null;
    public float? AvgSpeedMph => AvgSpeed.HasValue ? AvgSpeed.Value * 2.237f : null;
    public float? AvgSpeedKph => AvgSpeed.HasValue ? AvgSpeed.Value * 3.6f : null;
    public float? AvgPaceMinPerMile => AvgSpeed.HasValue && AvgSpeed.Value > 0 ? 26.8224f / AvgSpeed.Value : null;
    public float? AvgPaceMinPerKm => AvgSpeed.HasValue && AvgSpeed.Value > 0 ? 16.6667f / AvgSpeed.Value : null;
}
