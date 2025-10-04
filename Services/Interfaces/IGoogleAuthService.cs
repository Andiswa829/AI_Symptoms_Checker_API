using SymptomsCheckerApi.DTOs.Auth;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
    Task<AuthResponse> AuthenticateGoogleUserAsync(GoogleAuthRequest request);
}

public class GoogleUserInfo
{
    public string Sub { get; set; } = string.Empty; // Google user ID
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}