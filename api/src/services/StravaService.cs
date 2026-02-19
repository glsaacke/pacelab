using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using api.src.data;

namespace api.src.services;

public class StravaService : IStravaService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public StravaService(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public string GetAuthorizationUrl(int userId)
    {
        var clientId = _configuration["Strava:ClientId"]
            ?? Environment.GetEnvironmentVariable("Strava__ClientId");
        var redirectUri = _configuration["Strava:RedirectUri"]
            ?? Environment.GetEnvironmentVariable("Strava__RedirectUri");
        var authorizationUrl = _configuration["Strava:AuthorizationUrl"]
            ?? "https://www.strava.com/oauth/authorize";
        var scope = _configuration["Strava:Scope"] ?? "read,activity:read_all";

        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException("Strava:ClientId is not configured");
        }

        if (string.IsNullOrEmpty(redirectUri))
        {
            throw new InvalidOperationException("Strava:RedirectUri is not configured");
        }

        // Encode the userId in the state parameter so we can link the account on callback
        var state = userId.ToString();

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["response_type"] = "code",
            ["redirect_uri"] = redirectUri,
            ["approval_prompt"] = "auto",
            ["scope"] = scope,
            ["state"] = state
        };

        var queryString = string.Join("&", queryParams.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return $"{authorizationUrl}?{queryString}";
    }

    /// <inheritdoc />
    public async Task HandleCallbackAsync(string code, int userId)
    {
        var clientId = _configuration["Strava:ClientId"]
            ?? Environment.GetEnvironmentVariable("Strava__ClientId");
        var clientSecret = _configuration["Strava:ClientSecret"]
            ?? Environment.GetEnvironmentVariable("Strava__ClientSecret");
        var tokenUrl = _configuration["Strava:TokenUrl"]
            ?? "https://www.strava.com/oauth/token";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Strava client credentials are not configured");
        }

        // Exchange authorization code for tokens
        var tokenResponse = await ExchangeCodeForTokensAsync(tokenUrl, clientId, clientSecret, code);

        // Save tokens to the user record
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found");

        user.StravaUserId = tokenResponse.Athlete?.Id;
        user.StravaAccessToken = tokenResponse.AccessToken;
        user.StravaRefreshToken = tokenResponse.RefreshToken;
        // Strava returns expires_at as a Unix timestamp (seconds since epoch)
        user.StravaTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt).UtcDateTime;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<StravaTokenResponse> ExchangeCodeForTokensAsync(
        string tokenUrl, string clientId, string clientSecret, string code)
    {
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(tokenUrl, formData);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Strava token exchange failed ({(int)response.StatusCode}): {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<StravaTokenResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Strava returned an invalid token response");
        }

        return tokenResponse;
    }

    // Internal DTOs for Strava token response
    private sealed class StravaTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }

        [JsonPropertyName("athlete")]
        public StravaAthlete? Athlete { get; set; }
    }

    private sealed class StravaAthlete
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
