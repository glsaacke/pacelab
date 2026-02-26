namespace api.src.models.responses;

public class SyncTestResult
{
    public bool Success { get; set; }
    public bool StravaConnected { get; set; }
    public bool TokenValid { get; set; }
    public string? StravaUsername { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime TestedAt { get; set; }
}
