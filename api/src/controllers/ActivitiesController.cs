using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.src.services;
using api.src.models.requests;
using api.src.models.responses;

namespace api.src.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all activities for the authenticated user with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 20).</param>
    /// <param name="type">Optional activity type filter (e.g., "Run", "Ride").</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ActivityResponse>>> GetActivities(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? type = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var result = await _activityService.GetActivitiesAsync(userId, page, limit, type);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching activities for user");
            return StatusCode(500, new { error = "An error occurred while fetching activities" });
        }
    }

    /// <summary>
    /// Gets a single activity by ID.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityResponse>> GetActivity(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var activity = await _activityService.GetActivityByIdAsync(id, userId);

            if (activity == null)
            {
                return NotFound(new { error = "Activity not found" });
            }

            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching activity {ActivityId}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the activity" });
        }
    }

    /// <summary>
    /// Updates an activity's editable fields (name, type).
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="request">The update request.</param>
    [HttpPut("{id}")]
    public async Task<ActionResult<ActivityResponse>> UpdateActivity(int id, [FromBody] UpdateActivityRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var updatedActivity = await _activityService.UpdateActivityAsync(id, userId, request);

            if (updatedActivity == null)
            {
                return NotFound(new { error = "Activity not found" });
            }

            return Ok(updatedActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {ActivityId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the activity" });
        }
    }

    /// <summary>
    /// Deletes a single activity by ID.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteActivity(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var deleted = await _activityService.DeleteActivityAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new { error = "Activity not found" });
            }

            return Ok(new { message = "Activity deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {ActivityId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the activity" });
        }
    }

    /// <summary>
    /// Deletes all activities for the authenticated user.
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteAllActivities()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var deletedCount = await _activityService.DeleteAllActivitiesAsync(userId);

            return Ok(new 
            { 
                message = $"Successfully deleted {deletedCount} activities",
                deletedCount = deletedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all activities for user");
            return StatusCode(500, new { error = "An error occurred while deleting activities" });
        }
    }
}
