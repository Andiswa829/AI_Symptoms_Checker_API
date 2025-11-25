using SymptomsCheckerApi.DTOs.GoogleHealth;

namespace SymptomsCheckerApi.Services.Interfaces;

/// <summary>
/// Service for integrating with Google Health (Google Fit) API
/// </summary>
public interface IGoogleHealthService
{
    /// <summary>
    /// Sync all health data from Google Health for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Sync request with access token and date range</param>
    /// <returns>Comprehensive health data</returns>
    Task<GoogleHealthDataResponse> SyncHealthDataAsync(string userId, GoogleHealthSyncRequest request);
    
    /// <summary>
    /// Get heart rate data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Heart rate data</returns>
    Task<HeartRateData?> GetHeartRateDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get blood pressure data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Blood pressure data</returns>
    Task<BloodPressureData?> GetBloodPressureDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get oxygen saturation data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>SpO2 data</returns>
    Task<OxygenSaturationData?> GetOxygenSaturationDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get body temperature data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Temperature data</returns>
    Task<BodyTemperatureData?> GetBodyTemperatureDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get sleep data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Sleep data</returns>
    Task<SleepData?> GetSleepDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get activity and fitness data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Activity data</returns>
    Task<ActivityData?> GetActivityDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get respiratory rate data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Respiratory data</returns>
    Task<RespiratoryData?> GetRespiratoryDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get stress level and HRV data from Google Fit
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data</param>
    /// <param name="endDate">End date for data</param>
    /// <returns>Stress level data</returns>
    Task<StressLevelData?> GetStressLevelDataAsync(string accessToken, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Validate Google Health access token and scopes
    /// </summary>
    /// <param name="accessToken">Access token to validate</param>
    /// <returns>True if token is valid and has required scopes</returns>
    Task<bool> ValidateHealthTokenAsync(string accessToken);

    /// <summary>
    /// Refresh an access token using a Google refresh token
    /// Returns null if refresh fails
    /// </summary>
    /// <param name="refreshToken">Refresh token obtained with offline access</param>
    /// <returns>Tuple of access token and expires_in seconds, or null on failure</returns>
    Task<(string AccessToken, int ExpiresIn)?> RefreshAccessTokenAsync(string refreshToken);
}