namespace SymptomsCheckerApi.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = null!;
    public bool IsNewUser { get; set; } = false;
    public string? ExternalProvider { get; set; }
    
    /// <summary>
    /// Google access token for accessing Google APIs (like Google Fit)
    /// This token is used for health data access
    /// </summary>
    public string? GoogleAccessToken { get; set; }
    
    /// <summary>
    /// Expiration time for the Google access token
    /// </summary>
    public DateTime? GoogleAccessTokenExpiration { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePicture { get; set; }
    public bool IsExternalAuth { get; set; } = false;
}