using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.GoogleHealth;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

/// <summary>
/// Controller for Google Health integration endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoogleHealthController : ControllerBase
{
    private readonly IGoogleHealthService _googleHealthService;
    private readonly ILogger<GoogleHealthController> _logger;

    public GoogleHealthController(
        IGoogleHealthService googleHealthService,
        ILogger<GoogleHealthController> logger)
    {
        _googleHealthService = googleHealthService;
        _logger = logger;
    }

    /// <summary>
    /// 🏥 Sync all health data from Google Health
    /// </summary>
    /// <remarks>
    /// **📊 Comprehensive Health Data Sync**
    /// 
    /// Retrieves all available health metrics from Google Health (Google Fit) including:
    /// - ❤️ Heart rate (current, resting, max, min)
    /// - 🩺 Blood pressure readings
    /// - 🫁 Oxygen saturation (SpO2)
    /// - 🌡️ Body temperature
    /// - 😴 Sleep data and quality metrics
    /// - 🏃‍♂️ Activity and fitness data
    /// - 🫀 Respiratory rate (estimated)
    /// - 😰 Stress levels (estimated from HRV)
    /// 
    /// **🔑 Required:**
    /// - Valid Google OAuth access token with fitness scopes
    /// - Scopes needed: `https://www.googleapis.com/auth/fitness.read`
    /// 
    /// **📅 Date Range:**
    /// - Default: Last 7 days if not specified
    /// - Maximum: 1 year of historical data
    /// 
    /// **⚡ Performance:**
    /// - Parallel data fetching for optimal speed
    /// - Automatic error handling for missing data types
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "accessToken": "ya29.a0AfH6SMC...",
    ///   "startDate": "2025-09-28T00:00:00Z",
    ///   "endDate": "2025-10-05T23:59:59Z",
    ///   "dataTypes": ["heart_rate", "blood_pressure", "sleep"]
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Google Health sync request</param>
    /// <returns>Comprehensive health data from Google Health</returns>
    /// <response code="200">✅ Health data synced successfully</response>
    /// <response code="400">❌ Invalid request or access token</response>
    /// <response code="401">🔒 Unauthorized - invalid JWT token</response>
    /// <response code="403">🚫 Forbidden - insufficient Google Health permissions</response>
    /// <response code="500">💥 Internal server error</response>
    [HttpPost("sync")]
    public async Task<ActionResult<GoogleHealthDataResponse>> SyncHealthData([FromBody] GoogleHealthSyncRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            if (string.IsNullOrEmpty(request.AccessToken))
            {
                return BadRequest(new { message = "Google Health access token is required" });
            }

            // Validate the access token
            var isValidToken = await _googleHealthService.ValidateHealthTokenAsync(request.AccessToken);
            if (!isValidToken)
            {
                return BadRequest(new { 
                    message = "Invalid or expired Google Health access token",
                    details = "Please ensure you have a valid Google OAuth access token with the required fitness scopes",
                    requiredScopes = new[]
                    {
                        "https://www.googleapis.com/auth/fitness.activity.read",
                        "https://www.googleapis.com/auth/fitness.body.read",
                        "https://www.googleapis.com/auth/fitness.sleep.read",
                        "https://www.googleapis.com/auth/fitness.heart_rate.read",
                        "https://www.googleapis.com/auth/fitness.blood_pressure.read",
                        "https://www.googleapis.com/auth/fitness.oxygen_saturation.read"
                    }
                });
            }

            _logger.LogInformation("Syncing Google Health data for user {UserId}", userId);

            var healthData = await _googleHealthService.SyncHealthDataAsync(userId, request);

            return Ok(healthData);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to Google Health data");
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for Google Health sync");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Google Health data");
            return StatusCode(500, new { message = "An error occurred while syncing health data" });
        }
    }

    /// <summary>
    /// ❤️ Get heart rate data from Google Health
    /// </summary>
    /// <remarks>
    /// **💓 Heart Rate Monitoring**
    /// 
    /// Retrieves comprehensive heart rate data including:
    /// - Current heart rate (most recent reading)
    /// - Resting heart rate (lowest recorded)
    /// - Maximum and minimum heart rates
    /// - Historical readings with timestamps
    /// - Data source information
    /// 
    /// **📊 Data Includes:**
    /// - Real-time heart rate measurements
    /// - Exercise heart rate zones
    /// - Heart rate trends over time
    /// - Source device information (smartwatch, fitness tracker)
    /// 
    /// **🔍 Use Cases:**
    /// - Health monitoring and tracking
    /// - Fitness performance analysis
    /// - Medical condition monitoring
    /// - Symptom correlation analysis
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval (optional, defaults to 7 days ago)</param>
    /// <param name="endDate">End date for data retrieval (optional, defaults to now)</param>
    /// <returns>Heart rate data and statistics</returns>
    /// <response code="200">✅ Heart rate data retrieved successfully</response>
    /// <response code="400">❌ Invalid parameters</response>
    /// <response code="401">🔒 Unauthorized</response>
    /// <response code="404">📭 No heart rate data found</response>
    [HttpGet("heart-rate")]
    public async Task<ActionResult<HeartRateData>> GetHeartRateData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var heartRateData = await _googleHealthService.GetHeartRateDataAsync(accessToken, start, end);
            
            if (heartRateData == null)
            {
                return NotFound(new { message = "No heart rate data found for the specified period" });
            }

            return Ok(heartRateData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving heart rate data");
            return StatusCode(500, new { message = "Error retrieving heart rate data" });
        }
    }

    /// <summary>
    /// 🩺 Get blood pressure data from Google Health
    /// </summary>
    /// <remarks>
    /// **🫀 Blood Pressure Monitoring**
    /// 
    /// Retrieves blood pressure readings with medical categorization:
    /// - Latest systolic and diastolic readings
    /// - Blood pressure category (Normal, Elevated, High, Crisis)
    /// - Historical readings and trends
    /// - Source device and measurement context
    /// 
    /// **📋 Medical Categories:**
    /// - Normal: &lt;120 systolic and &lt;80 diastolic
    /// - Elevated: 120-129 systolic and &lt;80 diastolic  
    /// - High: ≥130 systolic or ≥80 diastolic
    /// - Crisis: ≥180 systolic or ≥120 diastolic
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Blood pressure readings and analysis</returns>
    [HttpGet("blood-pressure")]
    public async Task<ActionResult<BloodPressureData>> GetBloodPressureData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var bloodPressureData = await _googleHealthService.GetBloodPressureDataAsync(accessToken, start, end);
            
            if (bloodPressureData == null)
            {
                return NotFound(new { message = "No blood pressure data found" });
            }

            return Ok(bloodPressureData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blood pressure data");
            return StatusCode(500, new { message = "Error retrieving blood pressure data" });
        }
    }

    /// <summary>
    /// 🫁 Get oxygen saturation (SpO2) data from Google Health
    /// </summary>
    /// <remarks>
    /// **💨 Blood Oxygen Monitoring**
    /// 
    /// Retrieves oxygen saturation measurements:
    /// - Latest SpO2 percentage
    /// - Average oxygen saturation levels
    /// - Historical readings and trends
    /// - Normal range indicators (95-100%)
    /// 
    /// **🏥 Clinical Significance:**
    /// - Normal: 95-100% SpO2
    /// - Mild hypoxemia: 90-94% SpO2
    /// - Moderate hypoxemia: 85-89% SpO2
    /// - Severe hypoxemia: &lt;85% SpO2
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Oxygen saturation data and trends</returns>
    [HttpGet("oxygen-saturation")]
    public async Task<ActionResult<OxygenSaturationData>> GetOxygenSaturationData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var oxygenData = await _googleHealthService.GetOxygenSaturationDataAsync(accessToken, start, end);
            
            if (oxygenData == null)
            {
                return NotFound(new { message = "No oxygen saturation data found" });
            }

            return Ok(oxygenData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving oxygen saturation data");
            return StatusCode(500, new { message = "Error retrieving oxygen saturation data" });
        }
    }

    /// <summary>
    /// 🌡️ Get body temperature data from Google Health
    /// </summary>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Body temperature readings</returns>
    [HttpGet("body-temperature")]
    public async Task<ActionResult<BodyTemperatureData>> GetBodyTemperatureData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var temperatureData = await _googleHealthService.GetBodyTemperatureDataAsync(accessToken, start, end);
            
            if (temperatureData == null)
            {
                return NotFound(new { message = "No body temperature data found" });
            }

            return Ok(temperatureData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving body temperature data");
            return StatusCode(500, new { message = "Error retrieving body temperature data" });
        }
    }

    /// <summary>
    /// 😴 Get sleep data from Google Health
    /// </summary>
    /// <remarks>
    /// **🌙 Sleep Analysis & Quality**
    /// 
    /// Comprehensive sleep monitoring including:
    /// - Sleep duration and quality scores
    /// - Bedtime and wake time patterns
    /// - Sleep stages (Deep, Light, REM, Awake)
    /// - Sleep efficiency calculations
    /// - Weekly sleep statistics and trends
    /// 
    /// **📈 Quality Metrics:**
    /// - Sleep duration (recommended: 7-9 hours)
    /// - Sleep efficiency (time asleep vs time in bed)
    /// - Sleep consistency (regular sleep schedule)
    /// - Sleep stage distribution
    /// 
    /// **💡 Health Insights:**
    /// - Sleep debt calculations
    /// - Circadian rhythm analysis
    /// - Sleep quality trends
    /// - Recommendations for improvement
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Sleep data and quality analysis</returns>
    [HttpGet("sleep")]
    public async Task<ActionResult<SleepData>> GetSleepData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var sleepData = await _googleHealthService.GetSleepDataAsync(accessToken, start, end);
            
            if (sleepData == null)
            {
                return NotFound(new { message = "No sleep data found" });
            }

            return Ok(sleepData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sleep data");
            return StatusCode(500, new { message = "Error retrieving sleep data" });
        }
    }

    /// <summary>
    /// 🏃‍♂️ Get activity and fitness data from Google Health
    /// </summary>
    /// <remarks>
    /// **🎯 Activity & Fitness Tracking**
    /// 
    /// Comprehensive fitness monitoring:
    /// - Daily steps and step goals
    /// - Distance traveled and calories burned
    /// - Active minutes and exercise sessions
    /// - Activity type recognition (walking, running, cycling)
    /// - Fitness goals and achievement tracking
    /// 
    /// **📊 Metrics Included:**
    /// - Steps count and step rate
    /// - Distance (walking, running, cycling)
    /// - Calories burned (active + basal)
    /// - Active minutes and exercise duration
    /// - Heart rate zones during activities
    /// 
    /// **🎖️ Goals & Achievements:**
    /// - Daily step goals (recommended: 10,000 steps)
    /// - Active minutes goals (recommended: 150 min/week)
    /// - Calorie burn targets
    /// - Weekly activity summaries
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Activity and fitness data</returns>
    [HttpGet("activity")]
    public async Task<ActionResult<ActivityData>> GetActivityData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var activityData = await _googleHealthService.GetActivityDataAsync(accessToken, start, end);
            
            if (activityData == null)
            {
                return NotFound(new { message = "No activity data found" });
            }

            return Ok(activityData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity data");
            return StatusCode(500, new { message = "Error retrieving activity data" });
        }
    }

    /// <summary>
    /// 😰 Get stress level data from Google Health
    /// </summary>
    /// <remarks>
    /// **🧠 Stress Level Monitoring**
    /// 
    /// Advanced stress analysis using multiple metrics:
    /// - Current stress level (1-100 scale)
    /// - Stress category (Low, Moderate, High)
    /// - Heart rate variability (HRV) analysis
    /// - Stress patterns and triggers
    /// 
    /// **📈 Analysis Methods:**
    /// - Heart rate variability (HRV)
    /// - Resting heart rate trends
    /// - Sleep quality correlation
    /// - Activity level impact
    /// 
    /// **💡 Insights:**
    /// - Stress level categories and recommendations
    /// - Daily stress patterns
    /// - Recovery suggestions
    /// - Lifestyle factor correlations
    /// 
    /// **Note**: Stress levels are estimated from available biometric data
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token</param>
    /// <param name="startDate">Start date for data retrieval</param>
    /// <param name="endDate">End date for data retrieval</param>
    /// <returns>Stress level analysis and recommendations</returns>
    [HttpGet("stress-level")]
    public async Task<ActionResult<StressLevelData>> GetStressLevelData(
        [FromQuery] string accessToken,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var stressData = await _googleHealthService.GetStressLevelDataAsync(accessToken, start, end);
            
            if (stressData == null)
            {
                return NotFound(new { message = "No stress level data available" });
            }

            return Ok(stressData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stress level data");
            return StatusCode(500, new { message = "Error retrieving stress level data" });
        }
    }

    /// <summary>
    /// ✅ Validate Google Health access token
    /// </summary>
    /// <remarks>
    /// **🔐 Token Validation**
    /// 
    /// Validates Google OAuth access token for health data access:
    /// - Checks token validity and expiration
    /// - Verifies required fitness scopes
    /// - Tests API connectivity
    /// 
    /// **Required Scopes:**
    /// - `https://www.googleapis.com/auth/fitness.read`
    /// - `https://www.googleapis.com/auth/fitness.body.read`
    /// - `https://www.googleapis.com/auth/fitness.activity.read`
    /// - `https://www.googleapis.com/auth/fitness.sleep.read`
    /// 
    /// Use this endpoint before making health data requests to ensure proper authentication.
    /// </remarks>
    /// <param name="accessToken">Google OAuth access token to validate</param>
    /// <returns>Token validation result</returns>
    /// <response code="200">✅ Token is valid and has required scopes</response>
    /// <response code="400">❌ Invalid or expired token</response>
    /// <response code="403">🚫 Token lacks required fitness scopes</response>
    [HttpPost("validate-token")]
    public async Task<ActionResult> ValidateHealthToken([FromBody] string accessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest(new { message = "Access token is required" });
            }

            var isValid = await _googleHealthService.ValidateHealthTokenAsync(accessToken);
            
            if (isValid)
            {
                return Ok(new { 
                    valid = true, 
                    message = "Access token is valid and has required health scopes" 
                });
            }
            else
            {
                return BadRequest(new { 
                    valid = false, 
                    message = "Invalid access token or missing required health scopes" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating health access token");
            return StatusCode(500, new { message = "Error validating access token" });
        }
    }
}