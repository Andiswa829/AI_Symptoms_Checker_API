using Google.Apis.Auth.OAuth2;
using Google.Apis.Fitness.v1;
using Google.Apis.Fitness.v1.Data;
using Google.Apis.Services;
using SymptomsCheckerApi.DTOs.GoogleHealth;
using SymptomsCheckerApi.Services.Interfaces;
using System.Text.Json;

namespace SymptomsCheckerApi.Services;

/// <summary>
/// Service for integrating with Google Health (Google Fit) API
/// </summary>
public class GoogleHealthService : IGoogleHealthService
{
    private readonly ILogger<GoogleHealthService> _logger;
    private readonly HttpClient _httpClient;

    // Google Fit data type names
    private const string HeartRateDataType = "com.google.heart_rate.bpm";
    private const string BloodPressureDataType = "com.google.blood_pressure";
    private const string OxygenSaturationDataType = "com.google.oxygen_saturation";
    private const string BodyTemperatureDataType = "com.google.body.temperature";
    private const string SleepDataType = "com.google.sleep.segment";
    private const string StepsDataType = "com.google.step_count.delta";
    private const string CaloriesDataType = "com.google.calories.expended";
    private const string DistanceDataType = "com.google.distance.delta";
    private const string ActivityDataType = "com.google.activity.segment";
    private const string RespiratoryDataType = "com.google.heart_rate.bpm"; // Often derived from HR
    private const string StressDataType = "com.google.heart_rate.bpm"; // HRV analysis

    public GoogleHealthService(
        ILogger<GoogleHealthService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GoogleHealthDataResponse> SyncHealthDataAsync(string userId, GoogleHealthSyncRequest request)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-7);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            _logger.LogInformation("Syncing Google Health data for user {UserId} from {StartDate} to {EndDate}", 
                userId, startDate, endDate);

            var response = new GoogleHealthDataResponse
            {
                UserId = userId,
                SyncedAt = DateTime.UtcNow
            };

            // Sync all health data types in parallel for better performance
            var tasks = new List<Task>
            {
                Task.Run(async () => response.HeartRate = await GetHeartRateDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.BloodPressure = await GetBloodPressureDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.OxygenSaturation = await GetOxygenSaturationDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.BodyTemperature = await GetBodyTemperatureDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.Sleep = await GetSleepDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.Activity = await GetActivityDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.Respiratory = await GetRespiratoryDataAsync(request.AccessToken, startDate, endDate)),
                Task.Run(async () => response.StressLevel = await GetStressLevelDataAsync(request.AccessToken, startDate, endDate))
            };

            await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully synced Google Health data for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Google Health data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<HeartRateData?> GetHeartRateDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            var request = service.Users.DataSources.Datasets.Get("me", $"derived:com.google.heart_rate.bpm:com.google.android.gms:merge_heart_rate_bpm", $"{startTimeNanos}-{endTimeNanos}");
            var dataset = await request.ExecuteAsync();

            if (dataset.Point == null || !dataset.Point.Any())
                return null;

            var readings = new List<HeartRateReading>();
            var heartRates = new List<int>();

            foreach (var point in dataset.Point)
            {
                if (point.Value != null && point.Value.Any())
                {
                    var bpm = (int)point.Value.First().FpVal.GetValueOrDefault();
                    heartRates.Add(bpm);
                    
                    readings.Add(new HeartRateReading
                    {
                        Timestamp = NanosecondsToDateTime(point.StartTimeNanos.GetValueOrDefault()),
                        BeatsPerMinute = bpm,
                        Source = point.OriginDataSourceId ?? "Google Fit"
                    });
                }
            }

            if (!heartRates.Any())
                return null;

            return new HeartRateData
            {
                CurrentHeartRate = readings.OrderByDescending(r => r.Timestamp).FirstOrDefault()?.BeatsPerMinute,
                RestingHeartRate = heartRates.Min(),
                MaxHeartRate = heartRates.Max(),
                MinHeartRate = heartRates.Min(),
                Readings = readings.OrderByDescending(r => r.Timestamp).Take(20).ToList(),
                LastUpdated = readings.Max(r => r.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting heart rate data from Google Fit");
            return null;
        }
    }

    public async Task<BloodPressureData?> GetBloodPressureDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            var request = service.Users.DataSources.Datasets.Get("me", $"derived:{BloodPressureDataType}:com.google.android.gms:merged", $"{startTimeNanos}-{endTimeNanos}");
            var dataset = await request.ExecuteAsync();

            if (dataset.Point == null || !dataset.Point.Any())
                return null;

            var readings = new List<BloodPressureReading>();

            foreach (var point in dataset.Point)
            {
                if (point.Value != null && point.Value.Count >= 2)
                {
                    var systolic = (int)point.Value[0].FpVal.GetValueOrDefault();
                    var diastolic = (int)point.Value[1].FpVal.GetValueOrDefault();
                    
                    readings.Add(new BloodPressureReading
                    {
                        Timestamp = NanosecondsToDateTime(point.StartTimeNanos.GetValueOrDefault()),
                        Systolic = systolic,
                        Diastolic = diastolic,
                        Source = point.OriginDataSourceId ?? "Google Fit"
                    });
                }
            }

            if (!readings.Any())
                return null;

            var latest = readings.OrderByDescending(r => r.Timestamp).First();
            var category = GetBloodPressureCategory(latest.Systolic, latest.Diastolic);

            return new BloodPressureData
            {
                LatestSystolic = latest.Systolic,
                LatestDiastolic = latest.Diastolic,
                Category = category,
                Readings = readings.OrderByDescending(r => r.Timestamp).Take(10).ToList(),
                LastUpdated = readings.Max(r => r.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blood pressure data from Google Fit");
            return null;
        }
    }

    public async Task<OxygenSaturationData?> GetOxygenSaturationDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            var request = service.Users.DataSources.Datasets.Get("me", $"derived:{OxygenSaturationDataType}:com.google.android.gms:merged", $"{startTimeNanos}-{endTimeNanos}");
            var dataset = await request.ExecuteAsync();

            if (dataset.Point == null || !dataset.Point.Any())
                return null;

            var readings = new List<SpO2Reading>();
            var values = new List<double>();

            foreach (var point in dataset.Point)
            {
                if (point.Value != null && point.Value.Any())
                {
                    var spo2 = point.Value.First().FpVal.GetValueOrDefault();
                    values.Add(spo2);
                    
                    readings.Add(new SpO2Reading
                    {
                        Timestamp = NanosecondsToDateTime(point.StartTimeNanos.GetValueOrDefault()),
                        Percentage = spo2,
                        Source = point.OriginDataSourceId ?? "Google Fit"
                    });
                }
            }

            if (!readings.Any())
                return null;

            return new OxygenSaturationData
            {
                LatestSpO2 = readings.OrderByDescending(r => r.Timestamp).FirstOrDefault()?.Percentage,
                AverageSpO2 = values.Average(),
                Readings = readings.OrderByDescending(r => r.Timestamp).Take(10).ToList(),
                LastUpdated = readings.Max(r => r.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting oxygen saturation data from Google Fit");
            return null;
        }
    }

    public async Task<BodyTemperatureData?> GetBodyTemperatureDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            var request = service.Users.DataSources.Datasets.Get("me", $"derived:{BodyTemperatureDataType}:com.google.android.gms:merged", $"{startTimeNanos}-{endTimeNanos}");
            var dataset = await request.ExecuteAsync();

            if (dataset.Point == null || !dataset.Point.Any())
                return null;

            var readings = new List<TemperatureReading>();

            foreach (var point in dataset.Point)
            {
                if (point.Value != null && point.Value.Any())
                {
                    var temp = point.Value.First().FpVal.GetValueOrDefault();
                    
                    readings.Add(new TemperatureReading
                    {
                        Timestamp = NanosecondsToDateTime(point.StartTimeNanos.GetValueOrDefault()),
                        Temperature = temp,
                        Unit = "°C",
                        Source = point.OriginDataSourceId ?? "Google Fit"
                    });
                }
            }

            if (!readings.Any())
                return null;

            return new BodyTemperatureData
            {
                LatestTemperature = readings.OrderByDescending(r => r.Timestamp).FirstOrDefault()?.Temperature,
                TemperatureUnit = "°C",
                Readings = readings.OrderByDescending(r => r.Timestamp).Take(10).ToList(),
                LastUpdated = readings.Max(r => r.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting body temperature data from Google Fit");
            return null;
        }
    }

    public async Task<SleepData?> GetSleepDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            var request = service.Users.DataSources.Datasets.Get("me", $"derived:{SleepDataType}:com.google.android.gms:merged", $"{startTimeNanos}-{endTimeNanos}");
            var dataset = await request.ExecuteAsync();

            if (dataset.Point == null || !dataset.Point.Any())
                return null;

            var sessions = new List<SleepSession>();

            foreach (var point in dataset.Point)
            {
                var startTime = NanosecondsToDateTime(point.StartTimeNanos.GetValueOrDefault());
                var endTime = NanosecondsToDateTime(point.EndTimeNanos.GetValueOrDefault());
                var duration = endTime - startTime;

                sessions.Add(new SleepSession
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Duration = duration,
                    QualityScore = CalculateSleepQuality(duration), // Simple calculation
                    Stages = new List<SleepStage>() // Would need additional API calls for detailed stages
                });
            }

            if (!sessions.Any())
                return null;

            var lastNight = sessions.OrderByDescending(s => s.StartTime).FirstOrDefault();
            var avgDuration = TimeSpan.FromMilliseconds(sessions.Average(s => s.Duration.TotalMilliseconds));

            return new SleepData
            {
                LastNightSleepDuration = lastNight?.Duration,
                SleepQualityScore = lastNight?.QualityScore,
                BedTime = lastNight?.StartTime,
                WakeTime = lastNight?.EndTime,
                RecentSessions = sessions.OrderByDescending(s => s.StartTime).Take(7).ToList(),
                Statistics = new SleepStatistics
                {
                    AverageSleepDuration = avgDuration,
                    AverageQualityScore = (int)sessions.Average(s => s.QualityScore),
                    SleepEfficiencyPercentage = CalculateSleepEfficiency(sessions)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sleep data from Google Fit");
            return null;
        }
    }

    public async Task<ActivityData?> GetActivityDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var service = CreateFitnessService(accessToken);
            var startTimeNanos = DateTimeToNanoseconds(startDate);
            var endTimeNanos = DateTimeToNanoseconds(endDate);

            // Get steps data
            var stepsRequest = service.Users.DataSources.Datasets.Get("me", $"derived:{StepsDataType}:com.google.android.gms:estimated_steps", $"{startTimeNanos}-{endTimeNanos}");
            var stepsDataset = await stepsRequest.ExecuteAsync();

            // Get calories data
            var caloriesRequest = service.Users.DataSources.Datasets.Get("me", $"derived:{CaloriesDataType}:com.google.android.gms:merge_calories_expended", $"{startTimeNanos}-{endTimeNanos}");
            var caloriesDataset = await caloriesRequest.ExecuteAsync();

            // Get distance data
            var distanceRequest = service.Users.DataSources.Datasets.Get("me", $"derived:{DistanceDataType}:com.google.android.gms:merge_distance_delta", $"{startTimeNanos}-{endTimeNanos}");
            var distanceDataset = await distanceRequest.ExecuteAsync();

            var totalSteps = 0;
            var totalCalories = 0;
            var totalDistance = 0.0;

            // Process steps
            if (stepsDataset.Point != null)
            {
                foreach (var point in stepsDataset.Point)
                {
                    if (point.Value != null && point.Value.Any())
                    {
                        totalSteps += (int)point.Value.First().IntVal.GetValueOrDefault();
                    }
                }
            }

            // Process calories
            if (caloriesDataset.Point != null)
            {
                foreach (var point in caloriesDataset.Point)
                {
                    if (point.Value != null && point.Value.Any())
                    {
                        totalCalories += (int)point.Value.First().FpVal.GetValueOrDefault();
                    }
                }
            }

            // Process distance
            if (distanceDataset.Point != null)
            {
                foreach (var point in distanceDataset.Point)
                {
                    if (point.Value != null && point.Value.Any())
                    {
                        totalDistance += point.Value.First().FpVal.GetValueOrDefault();
                    }
                }
            }

            return new ActivityData
            {
                TodaySteps = totalSteps,
                TodayDistance = totalDistance / 1000, // Convert meters to kilometers
                TodayCaloriesBurned = totalCalories,
                TodayActiveMinutes = CalculateActiveMinutes(totalSteps), // Estimate based on steps
                RecentActivities = new List<ActivitySession>(), // Would need activity segments API
                Goals = new ActivityGoals
                {
                    DailyStepsGoal = 10000,
                    DailyActiveMinutesGoal = 30,
                    DailyCaloriesGoal = 2000
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity data from Google Fit");
            return null;
        }
    }

    public Task<RespiratoryData?> GetRespiratoryDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Note: Google Fit doesn't have direct respiratory rate data type
            // This is often estimated from heart rate variability or other sensors
            // For now, we'll return null or estimate from available data
            
            _logger.LogInformation("Respiratory rate data not directly available in Google Fit");
            return Task.FromResult<RespiratoryData?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting respiratory data from Google Fit");
            return Task.FromResult<RespiratoryData?>(null);
        }
    }

    public async Task<StressLevelData?> GetStressLevelDataAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Note: Google Fit doesn't have direct stress level data type
            // Stress is often estimated from heart rate variability
            // We could analyze heart rate data to estimate stress
            
            var heartRateData = await GetHeartRateDataAsync(accessToken, startDate, endDate);
            if (heartRateData?.Readings == null || !heartRateData.Readings.Any())
                return null;

            // Simple stress estimation based on heart rate variability
            var hrReadings = heartRateData.Readings;
            var avgHR = hrReadings.Average(r => r.BeatsPerMinute);
            var hrVariability = CalculateHRV(hrReadings);
            var stressLevel = EstimateStressFromHRV(hrVariability, avgHR);

            return new StressLevelData
            {
                CurrentStressLevel = stressLevel,
                StressCategory = GetStressCategory(stressLevel),
                HeartRateVariability = hrVariability,
                Readings = new List<StressReading>
                {
                    new StressReading
                    {
                        Timestamp = DateTime.UtcNow,
                        StressLevel = stressLevel,
                        HRV = hrVariability,
                        Source = "Estimated from HR data"
                    }
                },
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating stress level data");
            return null;
        }
    }

    public async Task<bool> ValidateHealthTokenAsync(string accessToken)
    {
        try
        {
            // Validate token by making a simple API call
            var service = CreateFitnessService(accessToken);
            var request = service.Users.DataSources.List("me");
            var response = await request.ExecuteAsync();
            
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google Health access token");
            return false;
        }
    }

    #region Private Helper Methods

    private FitnessService CreateFitnessService(string accessToken)
    {
        var credential = GoogleCredential.FromAccessToken(accessToken);
        return new FitnessService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Symptoms Checker API"
        });
    }

    private static long DateTimeToNanoseconds(DateTime dateTime)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(dateTime.ToUniversalTime() - unixEpoch).TotalMilliseconds * 1000000;
    }

    private static DateTime NanosecondsToDateTime(long nanoseconds)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return unixEpoch.AddMilliseconds(nanoseconds / 1000000.0);
    }

    private static string GetBloodPressureCategory(int systolic, int diastolic)
    {
        if (systolic >= 180 || diastolic >= 120) return "Hypertensive Crisis";
        if (systolic >= 140 || diastolic >= 90) return "High Blood Pressure";
        if (systolic >= 130 || diastolic >= 80) return "Elevated";
        if (systolic < 120 && diastolic < 80) return "Normal";
        return "Unknown";
    }

    private static int CalculateSleepQuality(TimeSpan duration)
    {
        // Simple sleep quality calculation based on duration
        var hours = duration.TotalHours;
        if (hours >= 7 && hours <= 9) return 85;
        if (hours >= 6 && hours <= 10) return 70;
        if (hours >= 5 && hours <= 11) return 55;
        return 40;
    }

    private static int CalculateSleepEfficiency(List<SleepSession> sessions)
    {
        if (!sessions.Any()) return 0;
        
        var avgDuration = sessions.Average(s => s.Duration.TotalHours);
        // Assume 8 hours is optimal
        return Math.Min(100, (int)((avgDuration / 8.0) * 100));
    }

    private static int CalculateActiveMinutes(int steps)
    {
        // Rough estimation: 2000 steps ≈ 20 minutes of activity
        return Math.Min(steps / 100, 300); // Cap at 300 minutes
    }

    private static double CalculateHRV(List<HeartRateReading> readings)
    {
        if (readings.Count < 2) return 0;
        
        var intervals = new List<double>();
        for (int i = 1; i < readings.Count; i++)
        {
            intervals.Add(Math.Abs(readings[i].BeatsPerMinute - readings[i - 1].BeatsPerMinute));
        }
        
        return intervals.Any() ? intervals.Average() : 0;
    }

    private static int EstimateStressFromHRV(double hrv, double avgHR)
    {
        // Simple stress estimation algorithm
        // Higher HRV generally indicates lower stress
        // Higher average HR can indicate higher stress
        
        var stressFromHRV = Math.Max(0, 100 - (hrv * 10)); // Lower HRV = higher stress
        var stressFromHR = Math.Min(100, Math.Max(0, (avgHR - 60) * 2)); // Higher HR = higher stress
        
        return (int)((stressFromHRV + stressFromHR) / 2);
    }

    private static string GetStressCategory(int stressLevel)
    {
        if (stressLevel <= 30) return "Low";
        if (stressLevel <= 60) return "Moderate";
        return "High";
    }

    #endregion
}