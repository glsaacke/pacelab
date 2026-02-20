using System;

namespace api.src.models.responses;

public class UserStravaStatus
{
    public bool Connected { get; set; }

    // Populated when Connected == true
    public long? StravaUserId { get; set; }
    public string? StravaUsername { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
}
