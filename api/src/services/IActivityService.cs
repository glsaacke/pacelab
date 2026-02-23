using api.src.models.requests;
using api.src.models.responses;

namespace api.src.services;

public interface IActivityService
{
    /// <summary>
    /// Gets all activities for a user with pagination support.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="limit">Number of items per page.</param>
    /// <param name="activityType">Optional filter by activity type (e.g., "Run", "Ride").</param>
    Task<PagedResponse<ActivityResponse>> GetActivitiesAsync(int userId, int page = 1, int limit = 20, string? activityType = null);

    /// <summary>
    /// Gets a single activity by ID. Ensures it belongs to the authenticated user.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<ActivityResponse?> GetActivityByIdAsync(int activityId, int userId);

    /// <summary>
    /// Updates an activity's editable fields (name, type).
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request">The update request with new values.</param>
    Task<ActivityResponse?> UpdateActivityAsync(int activityId, int userId, UpdateActivityRequest request);

    /// <summary>
    /// Deletes a single activity by ID.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<bool> DeleteActivityAsync(int activityId, int userId);

    /// <summary>
    /// Deletes all activities for a user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    Task<int> DeleteAllActivitiesAsync(int userId);
}
