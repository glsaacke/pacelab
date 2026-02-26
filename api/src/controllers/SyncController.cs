using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.src.services;
using api.src.models.responses;

namespace api.src.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the current sync status for the authenticated user, including
    /// Strava connection state, last sync time, and total activity count.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SyncStatusResponse>> GetStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var status = await _syncService.GetSyncStatusAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync status for user");
            return StatusCode(500, new { error = "An error occurred while fetching sync status" });
        }
    }

    /// <summary>
    /// Returns paginated sync history for the authenticated user.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Items per page (default: 20).</param>
    [HttpGet("history")]
    public async Task<ActionResult<PagedResponse<SyncHistoryResponse>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            var history = await _syncService.GetSyncHistoryAsync(userId, page, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync history for user");
            return StatusCode(500, new { error = "An error occurred while fetching sync history" });
        }
    }

    /// <summary>
    /// Tests the Strava connection for the authenticated user. Verifies tokens and makes a
    /// lightweight API call without persisting any data. Useful for validating the integration
    /// is healthy before triggering a full sync.
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult<SyncTestResult>> TestConnection()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var result = await _syncService.TestSyncConnectionAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing sync connection for user");
            return StatusCode(500, new { error = "An error occurred while testing the sync connection" });
        }
    }
}
