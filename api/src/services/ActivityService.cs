using Microsoft.EntityFrameworkCore;
using api.src.data;
using api.src.models.entities;
using api.src.models.requests;
using api.src.models.responses;

namespace api.src.services;

public class ActivityService : IActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(ApplicationDbContext context, ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<ActivityResponse>> GetActivitiesAsync(
        int userId, int page = 1, int limit = 20, string? activityType = null)
    {
        var query = _context.Activities.Where(a => a.UserId == userId);

        // Filter by activity type if provided
        if (!string.IsNullOrWhiteSpace(activityType))
        {
            query = query.Where(a => a.ActivityType == activityType);
        }

        // Get total count
        var total = await query.CountAsync();

        // Apply pagination and ordering (most recent first)
        var activities = await query
            .OrderByDescending(a => a.StartDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return new PagedResponse<ActivityResponse>
        {
            Data = activities.Select(MapToDto).ToList(),
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };
    }

    public async Task<ActivityResponse?> GetActivityByIdAsync(int activityId, int userId)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.ActivityId == activityId && a.UserId == userId);

        return activity != null ? MapToDto(activity) : null;
    }

    public async Task<ActivityResponse?> UpdateActivityAsync(
        int activityId, int userId, UpdateActivityRequest request)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.ActivityId == activityId && a.UserId == userId);

        if (activity == null)
        {
            return null;
        }

        // Update editable fields
        if (request.ActivityName != null)
        {
            activity.ActivityName = request.ActivityName;
        }

        if (request.ActivityType != null)
        {
            activity.ActivityType = request.ActivityType;
        }

        activity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(activity);
    }

    public async Task<bool> DeleteActivityAsync(int activityId, int userId)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.ActivityId == activityId && a.UserId == userId);

        if (activity == null)
        {
            return false;
        }

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteAllActivitiesAsync(int userId)
    {
        var activities = await _context.Activities
            .Where(a => a.UserId == userId)
            .ToListAsync();

        if (activities.Count == 0)
        {
            return 0;
        }

        _context.Activities.RemoveRange(activities);
        await _context.SaveChangesAsync();

        return activities.Count;
    }

    private static ActivityResponse MapToDto(Activity activity)
    {
        return new ActivityResponse
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
            AvgSpeed = activity.AvgSpeed,
            MaxSpeed = activity.MaxSpeed,
            TotalElevationGain = activity.TotalElevationGain,
            StartLatitude = activity.StartLatitude,
            StartLongitude = activity.StartLongitude,
            EndLatitude = activity.EndLatitude,
            EndLongitude = activity.EndLongitude,
            Polyline = activity.Polyline,
            AvgHeartrate = activity.AvgHeartrate,
            MaxHeartrate = activity.MaxHeartrate,
            AvgWatts = activity.AvgWatts,
            Calories = activity.Calories,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }
}
