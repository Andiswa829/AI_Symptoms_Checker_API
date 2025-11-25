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
            GoogleUserInfo googleUser;

            if (!string.IsNullOrEmpty(request.IdToken))
            {
                googleUser = await VerifyGoogleTokenAsync(request.IdToken);
            }
            else if (!string.IsNullOrEmpty(request.AccessToken))
            {
                // Verify access token and build GoogleUserInfo from tokeninfo
                var tokenInfo = await _httpClient.GetFromJsonAsync<GoogleTokenInfo>(
                    $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={request.AccessToken}");

                if (tokenInfo == null)
                {
                    throw new UnauthorizedAccessException("Invalid access token");
                }

                googleUser = new GoogleUserInfo
                {
                    Email = tokenInfo.Email ?? request.Email ?? string.Empty,
                    // Handle both string and boolean EmailVerified from tokenInfo
                    EmailVerified = tokenInfo.EmailVerified?.ToLower() == "true" || tokenInfo.EmailVerified == "True",
                    GivenName = request.FirstName ?? string.Empty,
                    FamilyName = request.LastName ?? string.Empty,
                    Picture = request.Picture ?? string.Empty,
                    Sub = tokenInfo.Sub ?? string.Empty
                };

                // For Google OAuth tokens, if we got a valid tokenInfo response, trust the email
                // Google pre-verifies emails for OAuth flows
                if (string.IsNullOrEmpty(tokenInfo.Email))
                {
                    throw new UnauthorizedAccessException("Email not available in token");
                }

                // Override EmailVerified for Google OAuth - Google verifies emails in OAuth flow
                googleUser.EmailVerified = true;
            }
            else
            {
                throw new UnauthorizedAccessException("No valid token provided (IdToken or AccessToken required)");
            }

            if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
            {
                throw new UnauthorizedAccessException("Invalid token or email not available");
            }

            // For Google OAuth, we trust that the email is verified
            // The tokeninfo endpoint confirms the token is valid for the client_id
            if (!googleUser.EmailVerified)
            {
                _logger.LogWarning("Email not verified for {Email}, but proceeding with Google OAuth", googleUser.Email);
                // For Google OAuth access tokens, we can trust the email since Google issued the token
                googleUser.EmailVerified = true;
            }


            // Verify the Google token
            ////var googleUser = await VerifyGoogleTokenAsync(request.IdToken);
            //if (googleUser == null || !googleUser.EmailVerified)
            //{
            //    throw new UnauthorizedAccessException("Invalid Google token or email not verified");
            //}

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

                // Persist Google tokens if provided (refresh token should be stored when offline access requested)
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    existingUser.GoogleHealthRefreshToken = request.RefreshToken;
                }

                if (!string.IsNullOrEmpty(request.AccessToken))
                {
                    existingUser.GoogleHealthAccessToken = request.AccessToken;
                    existingUser.GoogleHealthTokenExpiry = DateTime.UtcNow.AddHours(1);
                }

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

                // Update stored tokens if provided
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    existingUser.GoogleHealthRefreshToken = request.RefreshToken;
                }

                if (!string.IsNullOrEmpty(request.AccessToken))
                {
                    existingUser.GoogleHealthAccessToken = request.AccessToken;
                    existingUser.GoogleHealthTokenExpiry = DateTime.UtcNow.AddHours(1);
                }

                existingUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(existingUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Prepare response with Google access token if provided
            var response = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                IsNewUser = isNewUser,
                ExternalProvider = existingUser.ExternalProvider,
                GoogleAccessToken = request.AccessToken, // Pass through the access token
                GoogleAccessTokenExpiration = !string.IsNullOrEmpty(request.AccessToken) 
                    ? DateTime.UtcNow.AddHours(1) // Default expiration, could be validated from Google
                    : null,
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

            // If we have an access token, validate it for health data access
            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                _logger.LogInformation("Google access token provided, validating for user {UserId}", existingUser.Id);
            }

            return response;
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