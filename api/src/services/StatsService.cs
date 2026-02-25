using api.src.data;
using api.src.models.entities;
using api.src.extensions;
using Microsoft.EntityFrameworkCore;

namespace api.src.services;

public class StatsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StatsService> _logger;

    public StatsService(ApplicationDbContext context, ILogger<StatsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calculates adjusted speed for cycling activities based on weather and elevation.
    /// Fills in the ActivityAdjustments table for the specified activity.
    /// </summary>
    public async Task CalculateCyclingAdjustmentAsync(int activityId)
    {
        try
        {
            // Fetch activity
            var activity = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == activityId);
            if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found", activityId);
                return;
            }

            // Fetch weather data
            var weather = await _context.ActivityWeathers.FirstOrDefaultAsync(w => w.ActivityId == activityId);
            if (weather == null)
            {
                _logger.LogWarning("Weather data not found for activity {ActivityId}", activityId);
                return;
            }

            float rawSpeed = activity.AvgSpeed ?? 0; // m/s
            if (rawSpeed == 0)
            {
                _logger.LogWarning("Activity {ActivityId} has no average speed", activityId);
                return;
            }

            // Extract weather and activity data
            float windSpeed = weather.WindSpeedMps ?? 0;
            float temp = weather.TemperatureCelsius ?? 15;
            int humidity = weather.HumidityPercent ?? 50;
            float precip = weather.PrecipitationMm ?? 0;
            float elevationGain = activity.TotalElevationGain ?? 0;
            float distance = activity.DistanceMeters ?? 1;
            float altitude = 0; // TODO: Add altitude field to Activity if available

            // Calculate adjustments
            float windAdjustment = rawSpeed.CalculateWindAdjustment(windSpeed, 0.15f);
            float heatAdjustment = rawSpeed.CalculateHeatAdjustment(temp, humidity, 1.0f, 8);
            float coldAdjustment = rawSpeed.CalculateColdAdjustment(temp, 1.3f);
            float precipAdjustment = rawSpeed.CalculatePrecipAdjustment(precip, 1.3f);
            float elevationAdjustment = rawSpeed.CalculateElevationGainAdjustment(elevationGain, distance, 1.0f);
            float altitudeAdjustment = rawSpeed.CalculateAltitudeAdjustment(altitude);

            // Total adjustment
            float totalAdjustment = windAdjustment - heatAdjustment - coldAdjustment - precipAdjustment + elevationAdjustment - altitudeAdjustment;
            float adjustedSpeed = rawSpeed + totalAdjustment;
            float totalAdjustmentPercent = (totalAdjustment / rawSpeed) * 100;

            // Difficulty rating based on adjustments
            string difficultyRating = totalAdjustmentPercent.GetDifficultyRating();

            // Calculate adjusted time in seconds (distance / adjusted_speed)
            int adjustedTimeSeconds = adjustedSpeed > 0 ? (int)(distance / adjustedSpeed) : activity.MovingTimeSeconds ?? 0;

            // Check if adjustment record exists
            var existingAdjustment = await _context.ActivityAdjustments.FirstOrDefaultAsync(a => a.ActivityId == activityId);

            if (existingAdjustment != null)
            {
                // Update existing record
                existingAdjustment.AdjustedSpeedMps = adjustedSpeed;
                existingAdjustment.AdjustedTimeSeconds = adjustedTimeSeconds;
                existingAdjustment.WindAdjustment = windAdjustment;
                existingAdjustment.HeatAdjustment = heatAdjustment;
                existingAdjustment.ColdAdjustment = coldAdjustment;
                existingAdjustment.PrecipitationAdjustment = precipAdjustment;
                existingAdjustment.ElevationAdjustment = altitudeAdjustment;
                existingAdjustment.ElevationGainAdjustment = elevationAdjustment;
                existingAdjustment.TotalAdjustment = totalAdjustment;
                existingAdjustment.TotalAdjustmentPercent = totalAdjustmentPercent;
                existingAdjustment.DifficultyRating = difficultyRating;
                existingAdjustment.CalculatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new record
                var adjustment = new ActivityAdjustments
                {
                    ActivityId = activityId,
                    AdjustedSpeedMps = adjustedSpeed,
                    AdjustedTimeSeconds = adjustedTimeSeconds,
                    WindAdjustment = windAdjustment,
                    HeatAdjustment = heatAdjustment,
                    ColdAdjustment = coldAdjustment,
                    PrecipitationAdjustment = precipAdjustment,
                    ElevationAdjustment = altitudeAdjustment,
                    ElevationGainAdjustment = elevationAdjustment,
                    TotalAdjustment = totalAdjustment,
                    TotalAdjustmentPercent = totalAdjustmentPercent,
                    DifficultyRating = difficultyRating,
                    CalculatedAt = DateTime.UtcNow
                };
                _context.ActivityAdjustments.Add(adjustment);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cycling adjustment calculated for activity {ActivityId}: raw={RawSpeed}m/s, adjusted={AdjustedSpeed}m/s", activityId, rawSpeed, adjustedSpeed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cycling adjustment for activity {ActivityId}", activityId);
        }
    }

    /// <summary>
    /// Calculates adjusted pace for running activities based on weather and elevation.
    /// Result is converted to seconds per mile.
    /// </summary>
    public async Task CalculateRunningAdjustmentAsync(int activityId)
    {
        try
        {
            // Fetch activity
            var activity = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == activityId);
            if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found", activityId);
                return;
            }

            // Fetch weather data
            var weather = await _context.ActivityWeathers.FirstOrDefaultAsync(w => w.ActivityId == activityId);
            if (weather == null)
            {
                _logger.LogWarning("Weather data not found for activity {ActivityId}", activityId);
                return;
            }

            float rawSpeed = activity.AvgSpeed ?? 0; // m/s
            if (rawSpeed == 0)
            {
                _logger.LogWarning("Activity {ActivityId} has no average speed", activityId);
                return;
            }

            // Convert m/s to seconds per mile for the calculation
            float rawPaceSecPerMile = rawSpeed.ConvertMpsToSecPerMile();

            // Extract weather and activity data
            float windSpeed = weather.WindSpeedMps ?? 0;
            float temp = weather.TemperatureCelsius ?? 15;
            int humidity = weather.HumidityPercent ?? 50;
            float precip = weather.PrecipitationMm ?? 0;
            float elevationGain = activity.TotalElevationGain ?? 0;
            float distance = activity.DistanceMeters ?? 1;
            float altitude = 0; // TODO: Add altitude field to Activity if available

            // Calculate adjustments for running (working with pace in sec/mile)
            float windAdjustment = rawSpeed.CalculateWindAdjustment(windSpeed, 0.08f);
            float heatAdjustment = rawSpeed.CalculateHeatAdjustment(temp, humidity, 1.2f, 8);
            float coldAdjustment = rawSpeed.CalculateColdAdjustment(temp, 1.0f);
            float precipAdjustment = rawSpeed.CalculatePrecipAdjustment(precip, 1.0f);
            float elevationAdjustment = rawSpeed.CalculateElevationGainAdjustment(elevationGain, distance, 1.1f);
            float altitudeAdjustment = rawSpeed.CalculateAltitudeAdjustment(altitude);

            // Total adjustment
            float totalAdjustment = windAdjustment - heatAdjustment - coldAdjustment - precipAdjustment + elevationAdjustment - altitudeAdjustment;
            float adjustedSpeed = rawSpeed + totalAdjustment;
            float adjustedPaceSecPerMile = adjustedSpeed.ConvertMpsToSecPerMile();
            float totalAdjustmentPercent = (totalAdjustment / rawSpeed) * 100;

            // Difficulty rating based on adjustments
            string difficultyRating = totalAdjustmentPercent.GetDifficultyRating();

            // Calculate adjusted time in seconds (distance / adjusted_speed)
            int adjustedTimeSeconds = adjustedSpeed > 0 ? (int)(distance / adjustedSpeed) : activity.MovingTimeSeconds ?? 0;

            // Check if adjustment record exists
            var existingAdjustment = await _context.ActivityAdjustments.FirstOrDefaultAsync(a => a.ActivityId == activityId);

            if (existingAdjustment != null)
            {
                // Update existing record
                existingAdjustment.AdjustedSpeedMps = adjustedSpeed;
                existingAdjustment.AdjustedTimeSeconds = adjustedTimeSeconds;
                existingAdjustment.WindAdjustment = windAdjustment;
                existingAdjustment.HeatAdjustment = heatAdjustment;
                existingAdjustment.ColdAdjustment = coldAdjustment;
                existingAdjustment.PrecipitationAdjustment = precipAdjustment;
                existingAdjustment.ElevationAdjustment = altitudeAdjustment;
                existingAdjustment.ElevationGainAdjustment = elevationAdjustment;
                existingAdjustment.TotalAdjustment = totalAdjustment;
                existingAdjustment.TotalAdjustmentPercent = totalAdjustmentPercent;
                existingAdjustment.DifficultyRating = difficultyRating;
                existingAdjustment.CalculatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new record
                var adjustment = new ActivityAdjustments
                {
                    ActivityId = activityId,
                    AdjustedSpeedMps = adjustedSpeed,
                    AdjustedTimeSeconds = adjustedTimeSeconds,
                    WindAdjustment = windAdjustment,
                    HeatAdjustment = heatAdjustment,
                    ColdAdjustment = coldAdjustment,
                    PrecipitationAdjustment = precipAdjustment,
                    ElevationAdjustment = altitudeAdjustment,
                    ElevationGainAdjustment = elevationAdjustment,
                    TotalAdjustment = totalAdjustment,
                    TotalAdjustmentPercent = totalAdjustmentPercent,
                    DifficultyRating = difficultyRating,
                    CalculatedAt = DateTime.UtcNow
                };
                _context.ActivityAdjustments.Add(adjustment);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Running adjustment calculated for activity {ActivityId}: raw={RawSpeed}m/s ({RawPace}sec/mi), adjusted={AdjustedSpeed}m/s ({AdjustedPace}sec/mi)",
                activityId, rawSpeed, rawPaceSecPerMile, adjustedSpeed, adjustedPaceSecPerMile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating running adjustment for activity {ActivityId}", activityId);
        }
    }
}
