using api.src.models.responses;

namespace api.src.services;

public interface ISyncService
{
    /// <summary>
    /// Returns the current sync status for the user, including Strava connection state,
    /// last sync time, and total activity count.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<SyncStatusResponse> GetSyncStatusAsync(int userId);

    /// <summary>
    /// Returns paginated sync history for the user from the sync logs.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Maximum records per page.</param>
    Task<PagedResponse<SyncHistoryResponse>> GetSyncHistoryAsync(int userId, int page, int limit);

    /// <summary>
    /// Tests the Strava connection for the user by verifying tokens and making a lightweight
    /// API call without persisting any data. Useful for validating the integration is healthy.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<SyncTestResult> TestSyncConnectionAsync(int userId);
}
