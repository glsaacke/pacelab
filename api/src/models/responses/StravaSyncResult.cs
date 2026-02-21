namespace api.src.models.responses;

public class StravaSyncResult
{
    public int ActivitiesFound { get; set; }
    public int ActivitiesSkipped { get; set; }
    public int ActivitiesSynced { get; set; }
    public int WeatherFetched { get; set; }
    public int AdjustmentsCalculated { get; set; }
    public DateTime SyncedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
