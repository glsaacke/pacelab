namespace api.src.models.responses;

public class SyncStatusResponse
{
    public bool StravaConnected { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public int? LastSyncActivitiesSynced { get; set; }
    public string? LastSyncError { get; set; }
    public int TotalActivities { get; set; }
}
