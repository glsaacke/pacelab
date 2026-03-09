namespace api.src.models.responses;

public class SyncHistoryResponse
{
    public int SyncLogId { get; set; }
    public string? SyncType { get; set; }
    public string? Status { get; set; }
    public int? ActivitiesSynced { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? DurationSeconds { get; set; }
}
