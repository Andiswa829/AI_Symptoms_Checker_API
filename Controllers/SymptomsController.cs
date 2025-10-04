using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Symptoms;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SymptomsController : ControllerBase
{
    private readonly ISymptomAnalysisService _symptomAnalysisService;
    private readonly ILogger<SymptomsController> _logger;

    public SymptomsController(
        ISymptomAnalysisService symptomAnalysisService,
        ILogger<SymptomsController> logger)
    {
        _symptomAnalysisService = symptomAnalysisService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<SymptomAnalysisResponse>> AnalyzeSymptoms([FromBody] SymptomAnalysisRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _symptomAnalysisService.AnalyzeSymptomsAsync(request, userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing symptoms");
            return StatusCode(500, new { message = "An error occurred while analyzing symptoms" });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult> GetSymptomHistory()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var history = await _symptomAnalysisService.GetUserSymptomHistoryAsync(userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving symptom history");
            return StatusCode(500, new { message = "An error occurred while retrieving symptom history" });
        }
    }

    [HttpGet("{reportId}")]
    public async Task<ActionResult> GetSymptomReport(int reportId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var report = await _symptomAnalysisService.GetSymptomReportAsync(reportId, userId);
            if (report == null)
            {
                return NotFound();
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving symptom report {ReportId}", reportId);
            return StatusCode(500, new { message = "An error occurred while retrieving the symptom report" });
        }
    }
}