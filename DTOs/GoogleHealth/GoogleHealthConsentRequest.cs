using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.GoogleHealth;

/// <summary>
/// Request for user consent to sync Google health data
/// </summary>
public class GoogleHealthConsentRequest
{
    /// <summary>
    /// User's decision to sync health data
    /// </summary>
    [Required]
    public bool ConsentToSync { get; set; }
    
    /// <summary>
    /// Google OAuth access token (only required if ConsentToSync is true)
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Specific health data types user consented to sync
    /// </summary>
    public List<string> ConsentedDataTypes { get; set; } = new();
    
    /// <summary>
    /// User's preference for data sync frequency
    /// </summary>
    public string? SyncFrequency { get; set; } = "manual"; // manual, daily, weekly
}

/// <summary>
/// Response for health data sync consent
/// </summary>
public class GoogleHealthConsentResponse
{
    public bool ConsentRecorded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> AvailableDataTypes { get; set; } = new();
    public DateTime ConsentTimestamp { get; set; }
    public bool SyncCompleted { get; set; }
    public GoogleHealthDataResponse? HealthData { get; set; }
}