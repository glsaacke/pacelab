using api.src.models.responses;

namespace api.src.services;

public interface IStravaService
{
    /// <summary>
    /// Builds the Strava OAuth authorization URL to redirect the user to.
    /// </summary>
    /// <param name="userId">The authenticated user's ID, encoded in the state parameter.</param>
    /// <returns>The full Strava authorization URL.</returns>
    string GetAuthorizationUrl(int userId);

    /// <summary>
    /// Exchanges the authorization code from Strava's callback for access/refresh tokens,
    /// fetches the athlete profile, and saves the Strava tokens on the user record.
    /// </summary>
    /// <param name="code">The authorization code from Strava's OAuth callback.</param>
    /// <param name="userId">The authenticated user's ID decoded from the state parameter.</param>
    Task HandleCallbackAsync(string code, int userId);

    /// <summary>
    /// Disconnects the user's Strava account by removing all tokens and connection data.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task DisconnectAsync(int userId);

    /// <summary>
    /// Returns the current Strava connection status for the user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<UserStravaStatus> GetUserStatusAsync(int userId);

    /// <summary>
    /// Syncs recent activities from Strava for the authenticated user.
    /// Fetches activities, checks for duplicates, saves new ones with weather and adjustments.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<StravaSyncResult> SyncActivitiesAsync(int userId);
}
