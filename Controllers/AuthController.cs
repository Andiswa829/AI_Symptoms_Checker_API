using Google.Apis.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Auth;
using SymptomsCheckerApi.DTOs.Doctor;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services;
using SymptomsCheckerApi.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;
    private readonly IDoctorRegistrationService _doctorRegistrationService;
    private readonly HttpClient _httpClient = new HttpClient();


    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        IGoogleAuthService googleAuthService,
        ILogger<AuthController> logger, IDoctorRegistrationService doctorRegistrationService, HttpClient httpClient)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _googleAuthService = googleAuthService;
        _logger = logger;
        _doctorRegistrationService = doctorRegistrationService;
        _httpClient = httpClient;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Register as a doctor
    /// </summary>
    [HttpPost("register/doctor")]
    public async Task<ActionResult> RegisterDoctor([FromBody] DoctorRegistrationRequest request)
    {
        try
        {
            var (success, message, doctorId, userId) = await _doctorRegistrationService.RegisterDoctorAsync(request);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, doctorId, userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering doctor");
            return StatusCode(500, new { message = "An error occurred while registering as doctor" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    ProfilePicture = user.ProfilePicture,
                    IsExternalAuth = user.IsExternalAuth
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                ExternalProvider = user.ExternalProvider ?? string.Empty,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    ProfilePicture = user.ProfilePicture,
                    IsExternalAuth = user.IsExternalAuth,
                    IsDoctor = user.IsDoctor,
                    IsAdmin = user.IsAdmin
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// 🔐 Authenticate user with Google OAuth
    /// </summary>
    /// <remarks>
    /// **🌐 Google OAuth Authentication Flow:**
    /// 
    /// 1. **Frontend Setup**: Implement Google Sign-In in your frontend application
    /// 2. **Get ID Token**: User signs in with Google and you receive an ID token
    /// 3. **Get Access Token** (Optional): For health data access, also obtain an access token with fitness scopes
    /// 4. **API Call**: Send both tokens to this endpoint for authentication
    /// 
    /// **🔑 Required Scopes for Health Data:**
    /// - `https://www.googleapis.com/auth/fitness.body.read` - Body measurements
    /// - `https://www.googleapis.com/auth/fitness.activity.read` - Activity data  
    /// - `https://www.googleapis.com/auth/fitness.sleep.read` - Sleep data
    /// - `https://www.googleapis.com/auth/fitness.heart_rate.read` - Heart rate
    /// - `https://www.googleapis.com/auth/fitness.blood_pressure.read` - Blood pressure
    /// - `https://www.googleapis.com/auth/fitness.oxygen_saturation.read` - SpO2
    /// 
    /// **📝 Example Request:**
    /// ```json
    /// {
    ///   "idToken": "eyJhbGciOiJSUzI1NiIs...",
    ///   "accessToken": "ya29.a0AfH6SMC7X2vjf8nP9gK...", // For health data access
    ///   "email": "user@gmail.com",
    ///   "firstName": "John",
    ///   "lastName": "Doe",
    ///   "picture": "https://lh3.googleusercontent.com/a/default-user=s96-c"
    /// }
    /// ```
    /// 
    /// **🔑 Google Client ID**: `699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com`
    /// 
    /// **⚠️ Note**: The ID token must be obtained from Google's authentication service on the frontend.
    /// You cannot test this directly in Swagger without a real Google ID token from the frontend.
    /// </remarks>
    /// <param name="request">Google authentication request with ID token, optional access token, and user info</param>
    /// <returns>JWT authentication response with user details and optional Google access token</returns>
    /// <response code="200">✅ Authentication successful - returns JWT token and Google access token</response>
    /// <response code="400">❌ Invalid request or malformed ID token</response>
    /// <response code="401">🚫 Invalid Google ID token</response>
    /// <response code="500">💥 Internal server error</response>
    [HttpPost("googleLogin")]
    public async Task<ActionResult<AuthResponse>> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        try
        {
            // If idToken is null/empty, verify the accessToken instead
            if (string.IsNullOrEmpty(request.IdToken) && !string.IsNullOrEmpty(request.AccessToken))
            {
                // Verify access token with Google's tokeninfo endpoint
                var tokenInfo = await _httpClient.GetFromJsonAsync<GoogleTokenInfo>(
                    $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={request.AccessToken}");

                // Use tokenInfo.email, tokenInfo.email_verified, etc.
            }
            var response = await _googleAuthService.AuthenticateGoogleUserAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { message = "An error occurred during Google authentication" });
        }
    }

    /// <summary>
    /// 🔗 Generic external provider authentication (supports Google)
    /// </summary>
    /// <remarks>
    /// **🌐 External Authentication Options:**
    /// 
    /// Currently supports: **Google OAuth**
    /// 
    /// **📝 Example Request:**
    /// ```json
    /// {
    ///   "provider": "google",
    ///   "idToken": "eyJhbGciOiJSUzI1NiIs...",
    ///   "accessToken": "ya29.a0AfH6SMC7X2vjf8nP9gK...", 
    ///   "email": "user@gmail.com", 
    ///   "firstName": "John",
    ///   "lastName": "Doe",
    ///   "picture": "https://lh3.googleusercontent.com/a/default-user=s96-c"
    /// }
    /// ```
    /// 
    /// **🔑 For Google Health Data Access:**
    /// Include `accessToken` with the following scopes:
    /// - `https://www.googleapis.com/auth/fitness.body.read`
    /// - `https://www.googleapis.com/auth/fitness.activity.read`
    /// - `https://www.googleapis.com/auth/fitness.sleep.read`
    /// - `https://www.googleapis.com/auth/fitness.heart_rate.read`
    /// - `https://www.googleapis.com/auth/fitness.blood_pressure.read`
    /// - `https://www.googleapis.com/auth/fitness.oxygen_saturation.read`
    /// 
    /// **🔌 Supported Providers:**
    /// - `google` - Google OAuth 2.0
    /// 
    /// 🚀 Future providers may include: Facebook, Microsoft, Apple, etc.
    /// </remarks>
    /// <param name="request">External authentication request</param>
    /// <returns>JWT authentication response</returns>
    /// <response code="200">✅ Authentication successful</response>
    /// <response code="400">❌ Invalid provider or request</response>
    /// <response code="401">🚫 Authentication failed</response>
    /// <response code="500">💥 Internal server error</response>
    [HttpPost("external")]
    public async Task<ActionResult<AuthResponse>> ExternalAuth([FromBody] ExternalAuthRequest request)
    {
        try
        {
            if (request.Provider.ToLower() == "google")
            {
                var googleRequest = new GoogleAuthRequest
                {
                    IdToken = request.IdToken,
                    AccessToken = request.AccessToken, // Pass through the access token
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Picture = request.Picture
                };
                
                var response = await _googleAuthService.AuthenticateGoogleUserAsync(googleRequest);
                return Ok(response);
            }
            
            return BadRequest(new { message = $"Provider '{request.Provider}' is not supported" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during external authentication with provider {Provider}", request.Provider);
            return StatusCode(500, new { message = "An error occurred during external authentication" });
        }
    }
}