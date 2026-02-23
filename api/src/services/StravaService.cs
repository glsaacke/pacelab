using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using api.src.data;
using api.src.models.entities;
using api.src.models.responses;

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

        // Upsert the UserStravaConnection record
        var connection = await _context.UserStravaConnections
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (connection == null)
        {
            connection = new UserStravaConnection
            {
                UserId = userId,
                ConnectedAt = DateTime.UtcNow
            };
            _context.UserStravaConnections.Add(connection);
        }

        connection.StravaUserId = tokenResponse.Athlete?.Id;
        connection.StravaUsername = tokenResponse.Athlete?.Username;
        connection.StravaAccessToken = tokenResponse.AccessToken;
        connection.StravaRefreshToken = tokenResponse.RefreshToken;
        connection.StravaTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt).UtcDateTime;
        connection.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(int userId)
    {
        var connection = await _context.UserStravaConnections
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (connection != null)
        {
            _context.UserStravaConnections.Remove(connection);
            await _context.SaveChangesAsync();
        }
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

    public async Task<UserStravaStatus> GetUserStatusAsync(int userId)
    {
        var connection = await _context.UserStravaConnections
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (connection == null)
        {
            return new UserStravaStatus { Connected = false };
        }

        return new UserStravaStatus
        {
            Connected = true,
            StravaUserId = connection.StravaUserId,
            StravaUsername = connection.StravaUsername,
            TokenExpiresAt = connection.StravaTokenExpiresAt
        };
    }

    public async Task<StravaSyncResult> SyncActivitiesAsync(int userId)
    {
        var result = new StravaSyncResult
        {
            SyncedAt = DateTime.UtcNow
        };

        try
        {
            // 1. Verify user has Strava connected
            var connection = await _context.UserStravaConnections
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (connection == null)
            {
                throw new InvalidOperationException("Strava account is not connected");
            }

            // 2. Check/refresh Strava token if expired
            await RefreshTokenIfNeededAsync(connection);

            // 3. Fetch recent activities from Strava API
            var stravaActivities = await FetchStravaActivitiesAsync(connection.StravaAccessToken);
            result.ActivitiesFound = stravaActivities.Count;

            // Get user preferences for filtering
            var prefs = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            foreach (var stravaActivity in stravaActivities)
            {
                // Filter by activity type based on user preferences
                if (prefs != null)
                {
                    if (stravaActivity.Type == "Ride" && prefs.SyncCycling == false)
                    {
                        result.ActivitiesSkipped++;
                        continue;
                    }
                    if (stravaActivity.Type == "Run" && prefs.SyncRunning == false)
                    {
                        result.ActivitiesSkipped++;
                        continue;
                    }
                }

                // 4a. Check if already synced (skip duplicates)
                var exists = await _context.Activities
                    .AnyAsync(a => a.StravaActivityId == stravaActivity.Id);

                if (exists)
                {
                    result.ActivitiesSkipped++;
                    continue;
                }

                // 4b. Save activity to database
                var activity = new Activity
                {
                    UserId = userId,
                    StravaActivityId = stravaActivity.Id,
                    ActivityType = stravaActivity.Type,
                    ActivityName = stravaActivity.Name,
                    StartDate = stravaActivity.StartDate,
                    Timezone = stravaActivity.Timezone,
                    DistanceMeters = stravaActivity.Distance,
                    MovingTimeSeconds = stravaActivity.MovingTime,
                    ElapsedTimeSeconds = stravaActivity.ElapsedTime,
                    TotalElevationGain = stravaActivity.TotalElevationGain,
                    AvgSpeed = stravaActivity.AverageSpeed,
                    MaxSpeed = stravaActivity.MaxSpeed,
                    AvgHeartrate = (float?)stravaActivity.AverageHeartrate,
                    MaxHeartrate = stravaActivity.MaxHeartrate,
                    AvgWatts = stravaActivity.AverageWatts,
                    Calories = stravaActivity.Calories,
                    StartLatitude = stravaActivity.StartLatlng?[0],
                    StartLongitude = stravaActivity.StartLatlng?[1],
                    EndLatitude = stravaActivity.EndLatlng?[0],
                    EndLongitude = stravaActivity.EndLatlng?[1],
                    Polyline = stravaActivity.Map?.SummaryPolyline,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync(); // Save to get activity ID

                result.ActivitiesSynced++;

                // 4c. Fetch historical weather data (placeholder)
                if (activity.StartLatitude.HasValue && activity.StartLongitude.HasValue && activity.StartDate.HasValue)
                {
                    var weather = await FetchWeatherDataAsync(
                        activity.StartLatitude.Value,
                        activity.StartLongitude.Value,
                        activity.StartDate.Value);

                    if (weather != null)
                    {
                        weather.ActivityId = activity.ActivityId;
                        _context.ActivityWeathers.Add(weather);
                        result.WeatherFetched++;
                    }
                }

                // 4d. Calculate effort adjustments (placeholder)
                var adjustments = CalculateAdjustments(activity);
                if (adjustments != null)
                {
                    adjustments.ActivityId = activity.ActivityId;
                    _context.ActivityAdjustments.Add(adjustments);
                    result.AdjustmentsCalculated++;
                }

                await _context.SaveChangesAsync();
            }

            // 5. Update last sync timestamp
            connection.LastSync = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private async Task RefreshTokenIfNeededAsync(UserStravaConnection connection)
    {
        // Check if token expires within next hour
        if (connection.StravaTokenExpiresAt.HasValue &&
            connection.StravaTokenExpiresAt.Value > DateTime.UtcNow.AddHours(1))
        {
            return; // Token still valid
        }

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

        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = connection.StravaRefreshToken,
            ["grant_type"] = "refresh_token"
        });

        var response = await _httpClient.PostAsync(tokenUrl, formData);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to refresh Strava access token");
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<StravaTokenResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Invalid token refresh response");
        }

        connection.StravaAccessToken = tokenResponse.AccessToken;
        connection.StravaRefreshToken = tokenResponse.RefreshToken;
        connection.StravaTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt).UtcDateTime;
        connection.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<List<StravaActivity>> FetchStravaActivitiesAsync(string accessToken)
    {
        var url = "https://www.strava.com/api/v3/athlete/activities?per_page=30&page=1";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to fetch activities from Strava: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var activities = JsonSerializer.Deserialize<List<StravaActivity>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return activities ?? new List<StravaActivity>();
    }

    private async Task<ActivityWeather?> FetchWeatherDataAsync(float lat, float lon, DateTime date)
    {
        // TODO: Implement actual weather API integration (OpenWeather, WeatherAPI, etc.)
        // For now, return null - you'll implement this with your chosen weather provider
        await Task.CompletedTask;
        return null;
    }

    private ActivityAdjustments? CalculateAdjustments(Activity activity)
    {
        // TODO: Implement your effort adjustment algorithm
        // For now, return null - you'll implement the calculation logic
        return null;
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

        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private sealed class StravaActivity
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("distance")]
        public float Distance { get; set; }

        [JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }

        [JsonPropertyName("elapsed_time")]
        public int ElapsedTime { get; set; }

        [JsonPropertyName("total_elevation_gain")]
        public float TotalElevationGain { get; set; }

        [JsonPropertyName("average_speed")]
        public float AverageSpeed { get; set; }

        [JsonPropertyName("max_speed")]
        public float MaxSpeed { get; set; }

        [JsonPropertyName("average_heartrate")]
        public float? AverageHeartrate { get; set; }

        [JsonPropertyName("max_heartrate")]
        public float? MaxHeartrate { get; set; }

        [JsonPropertyName("average_watts")]
        public float? AverageWatts { get; set; }

        [JsonPropertyName("calories")]
        public float? Calories { get; set; }

        [JsonPropertyName("start_latlng")]
        public float[]? StartLatlng { get; set; }

        [JsonPropertyName("end_latlng")]
        public float[]? EndLatlng { get; set; }

        [JsonPropertyName("map")]
        public StravaMap? Map { get; set; }
    }

    private sealed class StravaMap
    {
        [JsonPropertyName("summary_polyline")]
        public string? SummaryPolyline { get; set; }
    }
}
