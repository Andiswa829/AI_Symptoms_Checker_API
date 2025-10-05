namespace SymptomsCheckerApi.DTOs.GoogleHealth;

/// <summary>
/// Request to sync Google Health data for a user
/// </summary>
public class GoogleHealthSyncRequest
{
    /// <summary>
    /// Google OAuth access token with fitness scopes
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date for data sync (optional, defaults to last 7 days)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date for data sync (optional, defaults to now)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Specific data types to sync (optional, defaults to all available)
    /// </summary>
    public List<string>? DataTypes { get; set; }
}