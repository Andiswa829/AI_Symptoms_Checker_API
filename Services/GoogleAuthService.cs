using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using SymptomsCheckerApi.DTOs.Auth;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;
using System.Text.Json;

namespace SymptomsCheckerApi.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly HttpClient _httpClient;

    public GoogleAuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<GoogleAuthService> logger,
        HttpClient httpClient)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            // Method 1: Using Google.Apis.Auth (more secure)
            var clientId = _configuration["Google:ClientId"];
            if (!string.IsNullOrEmpty(clientId))
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

                return new GoogleUserInfo
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }

            // Method 2: Fallback to Google's tokeninfo endpoint
            var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenInfo != null)
                {
                    return new GoogleUserInfo
                    {
                        Sub = tokenInfo.Sub,
                        Email = tokenInfo.Email,
                        Name = tokenInfo.Name,
                        GivenName = tokenInfo.GivenName,
                        FamilyName = tokenInfo.FamilyName,
                        Picture = tokenInfo.Picture,
                        EmailVerified = tokenInfo.EmailVerified == "true"
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Google token");
            return null;
        }
    }

    public async Task<AuthResponse> AuthenticateGoogleUserAsync(GoogleAuthRequest request)
    {
        try
        {
            // Verify the Google token
            var googleUser = await VerifyGoogleTokenAsync(request.IdToken);
            if (googleUser == null || !googleUser.EmailVerified)
            {
                throw new UnauthorizedAccessException("Invalid Google token or email not verified");
            }

            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);
            bool isNewUser = false;

            if (existingUser == null)
            {
                // Create new user
                existingUser = new User
                {
                    UserName = googleUser.Email,
                    Email = googleUser.Email,
                    FirstName = googleUser.GivenName,
                    LastName = googleUser.FamilyName,
                    ProfilePicture = googleUser.Picture,
                    IsExternalAuth = true,
                    ExternalProvider = "Google",
                    ExternalProviderId = googleUser.Sub,
                    EmailConfirmed = true, // Google emails are pre-verified
                    DateOfBirth = DateTime.MinValue // Will need to be updated later
                };

                var result = await _userManager.CreateAsync(existingUser);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                
                isNewUser = true;
            }
            else
            {
                // Update existing user's Google info if needed
                if (!existingUser.IsExternalAuth || existingUser.ExternalProvider != "Google")
                {
                    existingUser.IsExternalAuth = true;
                    existingUser.ExternalProvider = "Google";
                    existingUser.ExternalProviderId = googleUser.Sub;
                }
                
                // Update profile picture if provided and different
                if (!string.IsNullOrEmpty(googleUser.Picture) && existingUser.ProfilePicture != googleUser.Picture)
                {
                    existingUser.ProfilePicture = googleUser.Picture;
                }
                
                existingUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(existingUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                IsNewUser = isNewUser,
                User = new UserDto
                {
                    Id = existingUser.Id,
                    Email = existingUser.Email ?? string.Empty,
                    FirstName = existingUser.FirstName ?? string.Empty,
                    LastName = existingUser.LastName ?? string.Empty,
                    DateOfBirth = existingUser.DateOfBirth,
                    Gender = existingUser.Gender,
                    ProfilePicture = existingUser.ProfilePicture,
                    IsExternalAuth = existingUser.IsExternalAuth
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating Google user");
            throw;
        }
    }
}

// Helper class for Google token info response
internal class GoogleTokenInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string EmailVerified { get; set; } = string.Empty;
}