namespace SymptomsCheckerApi.DTOs.GoogleHealth;

/// <summary>
/// Comprehensive Google Health data response
/// </summary>
public class GoogleHealthDataResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Data sync timestamp
    /// </summary>
    public DateTime SyncedAt { get; set; }
    
    /// <summary>
    /// Heart rate data
    /// </summary>
    public HeartRateData? HeartRate { get; set; }
    
    /// <summary>
    /// Blood pressure readings
    /// </summary>
    public BloodPressureData? BloodPressure { get; set; }
    
    /// <summary>
    /// Oxygen saturation data
    /// </summary>
    public OxygenSaturationData? OxygenSaturation { get; set; }
    
    /// <summary>
    /// Body temperature readings
    /// </summary>
    public BodyTemperatureData? BodyTemperature { get; set; }
    
    /// <summary>
    /// Sleep data and quality
    /// </summary>
    public SleepData? Sleep { get; set; }
    
    /// <summary>
    /// Activity and fitness data
    /// </summary>
    public ActivityData? Activity { get; set; }
    
    /// <summary>
    /// Respiratory rate data
    /// </summary>
    public RespiratoryData? Respiratory { get; set; }
    
    /// <summary>
    /// Stress level indicators
    /// </summary>
    public StressLevelData? StressLevel { get; set; }
}

/// <summary>
/// Heart rate data with current and resting HR
/// </summary>
public class HeartRateData
{
    public int? CurrentHeartRate { get; set; }
    public int? RestingHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }
    public int? MinHeartRate { get; set; }
    public List<HeartRateReading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class HeartRateReading
{
    public DateTime Timestamp { get; set; }
    public int BeatsPerMinute { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Blood pressure data
/// </summary>
public class BloodPressureData
{
    public int? LatestSystolic { get; set; }
    public int? LatestDiastolic { get; set; }
    public List<BloodPressureReading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Category { get; set; } = string.Empty; // Normal, Elevated, High, etc.
}

public class BloodPressureReading
{
    public DateTime Timestamp { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Oxygen saturation (SpO2) data
/// </summary>
public class OxygenSaturationData
{
    public double? LatestSpO2 { get; set; }
    public double? AverageSpO2 { get; set; }
    public List<SpO2Reading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class SpO2Reading
{
    public DateTime Timestamp { get; set; }
    public double Percentage { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Body temperature data
/// </summary>
public class BodyTemperatureData
{
    public double? LatestTemperature { get; set; }
    public string TemperatureUnit { get; set; } = "°C";
    public List<TemperatureReading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class TemperatureReading
{
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public string Unit { get; set; } = "°C";
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Sleep data and quality metrics
/// </summary>
public class SleepData
{
    public TimeSpan? LastNightSleepDuration { get; set; }
    public int? SleepQualityScore { get; set; }
    public DateTime? BedTime { get; set; }
    public DateTime? WakeTime { get; set; }
    public List<SleepSession> RecentSessions { get; set; } = new();
    public SleepStatistics Statistics { get; set; } = new();
}

public class SleepSession
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int QualityScore { get; set; }
    public List<SleepStage> Stages { get; set; } = new();
}

public class SleepStage
{
    public string Stage { get; set; } = string.Empty; // Deep, Light, REM, Awake
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
}

public class SleepStatistics
{
    public TimeSpan AverageSleepDuration { get; set; }
    public int AverageQualityScore { get; set; }
    public int SleepEfficiencyPercentage { get; set; }
}

/// <summary>
/// Activity and fitness data
/// </summary>
public class ActivityData
{
    public int? TodaySteps { get; set; }
    public double? TodayDistance { get; set; } // in kilometers
    public int? TodayCaloriesBurned { get; set; }
    public int? TodayActiveMinutes { get; set; }
    public List<ActivitySession> RecentActivities { get; set; } = new();
    public ActivityGoals Goals { get; set; } = new();
}

public class ActivitySession
{
    public string ActivityType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public int Steps { get; set; }
    public double Distance { get; set; }
}

public class ActivityGoals
{
    public int DailyStepsGoal { get; set; }
    public int DailyActiveMinutesGoal { get; set; }
    public int DailyCaloriesGoal { get; set; }
}

/// <summary>
/// Respiratory rate and breathing data
/// </summary>
public class RespiratoryData
{
    public int? LatestRespiratoryRate { get; set; } // breaths per minute
    public int? AverageRespiratoryRate { get; set; }
    public List<RespiratoryReading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class RespiratoryReading
{
    public DateTime Timestamp { get; set; }
    public int BreathsPerMinute { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Stress level and HRV data
/// </summary>
public class StressLevelData
{
    public int? CurrentStressLevel { get; set; } // 1-100 scale
    public string StressCategory { get; set; } = string.Empty; // Low, Moderate, High
    public double? HeartRateVariability { get; set; }
    public List<StressReading> Readings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class StressReading
{
    public DateTime Timestamp { get; set; }
    public int StressLevel { get; set; }
    public double? HRV { get; set; }
    public string Source { get; set; } = string.Empty;
}