namespace api.src.models.requests;

public class CreateTestActivityRequest
{
    public int UserId { get; set; }
    
    // Optional fields with sensible defaults
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
}
