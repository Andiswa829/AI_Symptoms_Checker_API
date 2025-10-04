using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Health;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthController : ControllerBase
{
    private readonly IHealthDataService _healthDataService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IHealthDataService healthDataService,
        ILogger<HealthController> logger)
    {
        _healthDataService = healthDataService;
        _logger = logger;
    }

    [HttpGet("data")]
    public async Task<ActionResult<HealthData>> GetHealthData()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var healthData = await _healthDataService.GetHealthDataAsync(userId);
            if (healthData == null)
            {
                return NotFound();
            }

            return Ok(healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health data");
            return StatusCode(500, new { message = "An error occurred while retrieving health data" });
        }
    }

    [HttpPut("data")]
    public async Task<ActionResult<HealthData>> UpdateHealthData([FromBody] HealthDataRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var healthData = await _healthDataService.UpdateHealthDataAsync(request, userId);
            return Ok(healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating health data");
            return StatusCode(500, new { message = "An error occurred while updating health data" });
        }
    }
}