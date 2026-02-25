namespace api.src.extensions;

/// <summary>
/// Extension methods for activity adjustment calculations.
/// These methods extend the float type to provide reusable calculation logic.
/// </summary>
public static class AdjustmentExtensions
{
    /// <summary>
    /// Calculates wind adjustment component.
    /// Formula: (wind_speed * 0.6) * coefficient * (1 + raw_speed/10)
    /// </summary>
    public static float CalculateWindAdjustment(this float rawSpeed, float windSpeed, float coefficient)
    {
        return (windSpeed * 0.6f) * coefficient * (1 + rawSpeed / 10f);
    }

    /// <summary>
    /// Calculates heat adjustment component (applied when temp > 20°C).
    /// Formula: raw_speed * (temp - 15) * 0.008 * clamp(0.8, 1.3, 1 + (humidity - 50)/200) * multiplier * (1 + 8/raw_speed)
    /// </summary>
    public static float CalculateHeatAdjustment(this float rawSpeed, float temp, int humidity, float multiplier, float division)
    {
        if (temp <= 20)
            return 0;

        float humidityFactor = Clamp(0.8f, 1.3f, 1 + (humidity - 50) / 200f);
        return rawSpeed * (temp - 15) * 0.008f * humidityFactor * multiplier * (1 + division / rawSpeed);
    }

    /// <summary>
    /// Calculates cold adjustment component (applied when temp < 5°C).
    /// Formula: raw_speed * (10 - temp) * 0.005 * multiplier * (1 + raw_speed/15)
    /// </summary>
    public static float CalculateColdAdjustment(this float rawSpeed, float temp, float multiplier)
    {
        if (temp >= 5)
            return 0;

        return rawSpeed * (10 - temp) * 0.005f * multiplier * (1 + rawSpeed / 15f);
    }

    /// <summary>
    /// Calculates precipitation adjustment component (applied when precip > 0.1mm).
    /// Formula: raw_speed * (precip < 2 ? 0.02 : precip < 5 ? 0.05 : 0.10) * multiplier
    /// </summary>
    public static float CalculatePrecipAdjustment(this float rawSpeed, float precip, float multiplier)
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
    public static float CalculateElevationGainAdjustment(this float rawSpeed, float elevationGain, float distance, float multiplier)
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
    public static float CalculateAltitudeAdjustment(this float rawSpeed, float altitude)
    {
        if (altitude <= 1500)
            return 0;

        return rawSpeed * ((altitude - 1500) / 300f) * 0.03f;
    }

    /// <summary>
    /// Clamps a value between min and max.
    /// </summary>
    public static float Clamp(this float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Converts speed in m/s to pace in seconds per mile.
    /// </summary>
    public static float ConvertMpsToSecPerMile(this float mps)
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
    public static string GetDifficultyRating(this float totalAdjustmentPercent)
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
