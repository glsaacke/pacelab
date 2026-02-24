using api.src.data;
using api.src.models.entities;
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
            float windAdjustment = CalculateWindAdjustment(rawSpeed, windSpeed, 0.15f);
            float heatAdjustment = CalculateHeatAdjustment(rawSpeed, temp, humidity, 1.0f, 8);
            float coldAdjustment = CalculateColdAdjustment(rawSpeed, temp, 1.3f);
            float precipAdjustment = CalculatePrecipAdjustment(rawSpeed, precip, 1.3f);
            float elevationAdjustment = CalculateElevationGainAdjustment(rawSpeed, elevationGain, distance, 1.0f);
            float altitudeAdjustment = CalculateAltitudeAdjustment(rawSpeed, altitude);

            // Total adjustment
            float totalAdjustment = windAdjustment - heatAdjustment - coldAdjustment - precipAdjustment + elevationAdjustment - altitudeAdjustment;
            float adjustedSpeed = rawSpeed + totalAdjustment;
            float totalAdjustmentPercent = (totalAdjustment / rawSpeed) * 100;

            // Difficulty rating based on adjustments
            string difficultyRating = GetDifficultyRating(totalAdjustmentPercent);

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
            float rawPaceSecPerMile = ConvertMpsToSecPerMile(rawSpeed);

            // Extract weather and activity data
            float windSpeed = weather.WindSpeedMps ?? 0;
            float temp = weather.TemperatureCelsius ?? 15;
            int humidity = weather.HumidityPercent ?? 50;
            float precip = weather.PrecipitationMm ?? 0;
            float elevationGain = activity.TotalElevationGain ?? 0;
            float distance = activity.DistanceMeters ?? 1;
            float altitude = 0; // TODO: Add altitude field to Activity if available

            // Calculate adjustments for running (working with pace in sec/mile)
            float windAdjustment = CalculateWindAdjustment(rawSpeed, windSpeed, 0.08f);
            float heatAdjustment = CalculateHeatAdjustment(rawSpeed, temp, humidity, 1.2f, 8);
            float coldAdjustment = CalculateColdAdjustment(rawSpeed, temp, 1.0f);
            float precipAdjustment = CalculatePrecipAdjustment(rawSpeed, precip, 1.0f);
            float elevationAdjustment = CalculateElevationGainAdjustment(rawSpeed, elevationGain, distance, 1.1f);
            float altitudeAdjustment = CalculateAltitudeAdjustment(rawSpeed, altitude);

            // Total adjustment
            float totalAdjustment = windAdjustment - heatAdjustment - coldAdjustment - precipAdjustment + elevationAdjustment - altitudeAdjustment;
            float adjustedSpeed = rawSpeed + totalAdjustment;
            float adjustedPaceSecPerMile = ConvertMpsToSecPerMile(adjustedSpeed);
            float totalAdjustmentPercent = (totalAdjustment / rawSpeed) * 100;

            // Difficulty rating based on adjustments
            string difficultyRating = GetDifficultyRating(totalAdjustmentPercent);

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

    /// <summary>
    /// Calculates wind adjustment component.
    /// Formula: (wind_speed * 0.6) * coefficient * (1 + raw_speed/10)
    /// </summary>
    private float CalculateWindAdjustment(float rawSpeed, float windSpeed, float coefficient)
    {
        return (windSpeed * 0.6f) * coefficient * (1 + rawSpeed / 10f);
    }

    /// <summary>
    /// Calculates heat adjustment component (applied when temp > 20°C).
    /// Formula: raw_speed * (temp - 15) * 0.008 * clamp(0.8, 1.3, 1 + (humidity - 50)/200) * multiplier * (1 + 8/raw_speed)
    /// </summary>
    private float CalculateHeatAdjustment(float rawSpeed, float temp, int humidity, float multiplier, float division)
    {
        if (temp <= 20)
            return 0;

        float humidityFactor = Clamp(0.8f, 1.3f, 1 + (humidity - 50) / 200f);
        return rawSpeed * (temp - 15) * 0.008f * humidityFactor * multiplier * (1 + division / rawSpeed);
    }

    /// <summary>
    /// Calculates cold adjustment component (applied when temp < 5°C).
    /// Formula: raw_speed * (10 - temp) * 0.005 * multiplier * (1 + raw_speed/15 for cycling, 1.0 for running)
    /// </summary>
    private float CalculateColdAdjustment(float rawSpeed, float temp, float multiplier)
    {
        if (temp >= 5)
            return 0;

        return rawSpeed * (10 - temp) * 0.005f * multiplier * (1 + rawSpeed / 15f);
    }

    /// <summary>
    /// Calculates precipitation adjustment component (applied when precip > 0.1mm).
    /// Formula: raw_speed * (precip < 2 ? 0.02 : precip < 5 ? 0.05 : 0.10) * multiplier
    /// </summary>
    private float CalculatePrecipAdjustment(float rawSpeed, float precip, float multiplier)
    {
        if (precip <= 0.1f)
            return 0;

        float precipFactor = precip < 2 ? 0.02f : precip < 5 ? 0.05f : 0.10f;
        return rawSpeed * precipFactor * multiplier;
    }

    /// <summary>
    /// Calculates elevation gain adjustment component (applied when elevation_gain > 10m).
    /// Formula: raw_speed * (elevation_gain * 10 / distance) * multiplier
    /// </summary>
    private float CalculateElevationGainAdjustment(float rawSpeed, float elevationGain, float distance, float multiplier)
    {
        if (elevationGain <= 10)
            return 0;

        if (distance == 0)
            return 0;

        return rawSpeed * (elevationGain * 10 / distance) * multiplier;
    }

    /// <summary>
    /// Calculates altitude adjustment component (applied when altitude > 1500m).
    /// Formula: raw_speed * ((altitude - 1500) / 300) * 0.03
    /// </summary>
    private float CalculateAltitudeAdjustment(float rawSpeed, float altitude)
    {
        if (altitude <= 1500)
            return 0;

        return rawSpeed * ((altitude - 1500) / 300f) * 0.03f;
    }

    /// <summary>
    /// Clamps a value between min and max.
    /// </summary>
    private float Clamp(float min, float max, float value)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Converts speed in m/s to pace in seconds per mile.
    /// </summary>
    private float ConvertMpsToSecPerMile(float mps)
    {
        if (mps == 0)
            return 0;

        // 1 mile = 1609.34 meters
        // pace (sec/mile) = 1609.34 / speed (m/s)
        return 1609.34f / mps;
    }

    /// <summary>
    /// Determines difficulty rating based on total adjustment percentage.
    /// </summary>
    private string GetDifficultyRating(float totalAdjustmentPercent)
    {
        return totalAdjustmentPercent switch
        {
            < -15 => "Very Easy",
            < -5 => "Easy",
            < 5 => "Normal",
            < 15 => "Challenging",
            >= 15 => "Very Challenging",
            _ => "Normal"
        };
    }
}
