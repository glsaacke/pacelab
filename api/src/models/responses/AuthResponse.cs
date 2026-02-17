namespace api.src.models.responses;

public class AuthResponse
{
    public UserResponse User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class UserResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoggedIn { get; set; }
}
