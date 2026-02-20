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
}
