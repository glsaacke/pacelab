using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.src.data;
using api.src.models.entities;
using api.src.models.requests;
using api.src.models.responses;
using api.src.services;

namespace api.src.controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IActivityService activityService, ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _activityService = activityService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("CreateTestActivity")]
    public async Task<ActionResult<ActivityResponse>> CreateTestActivity([FromQuery] int userId,[FromBody] CreateTestActivityRequest request)
    {
        try
        {

            // Create a generic test activity
            var activity = new Activity
            {
                UserId = userId,
                StravaActivityId = new Random().NextInt64(1000000000, 9999999999), // Random Strava ID for testing
                ActivityType = request.ActivityType ?? "Run",
                ActivityName = request.ActivityName ?? "Test Activity",
                StartDate = request.StartDate ?? DateTime.UtcNow.AddDays(-1),
                Timezone = request.Timezone ?? "UTC",
                DistanceMeters = request.DistanceMeters ?? 5000f, // 5km default
                MovingTimeSeconds = request.MovingTimeSeconds ?? 1800, // 30 minutes default
                ElapsedTimeSeconds = request.ElapsedTimeSeconds ?? 2000,
                TotalElevationGain = request.TotalElevationGain ?? 100f,
                AvgSpeed = request.AvgSpeed ?? 2.78f, // ~10 km/h
                MaxSpeed = request.MaxSpeed ?? 5.0f,
                AvgHeartrate = request.AvgHeartrate,
                MaxHeartrate = request.MaxHeartrate,
                AvgWatts = request.AvgWatts,
                Calories = request.Calories,
                StartLatitude = request.StartLatitude ?? 40.7128f,
                StartLongitude = request.StartLongitude ?? -74.0060f,
                EndLatitude = request.EndLatitude ?? 40.7260f,
                EndLongitude = request.EndLongitude ?? -74.0090f,
                Polyline = request.Polyline,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created test activity {ActivityId} for user {UserId}", activity.ActivityId, request.UserId);

            // Map to DTO and return
            var response = new ActivityResponse
            {
                ActivityId = activity.ActivityId,
                UserId = activity.UserId,
                StravaActivityId = activity.StravaActivityId,
                ActivityType = activity.ActivityType,
                ActivityName = activity.ActivityName,
                StartDate = activity.StartDate,
                Timezone = activity.Timezone,
                DistanceMeters = activity.DistanceMeters,
                MovingTimeSeconds = activity.MovingTimeSeconds,
                ElapsedTimeSeconds = activity.ElapsedTimeSeconds,
                TotalElevationGain = activity.TotalElevationGain,
                AvgSpeed = activity.AvgSpeed,
                MaxSpeed = activity.MaxSpeed,
                AvgHeartrate = activity.AvgHeartrate,
                MaxHeartrate = activity.MaxHeartrate,
                AvgWatts = activity.AvgWatts,
                Calories = activity.Calories,
                StartLatitude = activity.StartLatitude,
                StartLongitude = activity.StartLongitude,
                EndLatitude = activity.EndLatitude,
                EndLongitude = activity.EndLongitude,
                Polyline = activity.Polyline,
                CreatedAt = activity.CreatedAt,
                UpdatedAt = activity.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test activity for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "Failed to create test activity" });
        }
    }
}
