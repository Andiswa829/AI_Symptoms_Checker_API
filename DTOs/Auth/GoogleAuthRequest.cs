using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Auth;

public class GoogleAuthRequest
{
    public string IdToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Google OAuth access token for accessing Google APIs (like Google Fit)
    /// This is different from the ID token and is used for API access
    /// </summary>
    public string? AccessToken { get; set; }
    /// <summary>
    /// Refresh token returned when requesting offline access (access_type=offline)
    /// Store this on the server to refresh access tokens when they expire.
    /// </summary>
    public string? RefreshToken { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Picture { get; set; }
}