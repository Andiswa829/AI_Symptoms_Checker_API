using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Outbreaks;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OutbreaksController : ControllerBase
{
    private readonly IOutbreakService _outbreakService;
    private readonly ILogger<OutbreaksController> _logger;

    public OutbreaksController(
        IOutbreakService outbreakService,
        ILogger<OutbreaksController> logger)
    {
        _outbreakService = outbreakService;
        _logger = logger;
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<OutbreakAlertResponse>>> GetActiveOutbreaks(
        [FromQuery] string? location = null)
    {
        try
        {
            var outbreaks = await _outbreakService.GetActiveOutbreaksAsync(location);
            return Ok(outbreaks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active outbreaks");
            return StatusCode(500, new { message = "An error occurred while retrieving outbreak alerts" });
        }
    }

    [HttpGet("{outbreakId}")]
    public async Task<ActionResult<OutbreakAlertResponse>> GetOutbreakDetails(int outbreakId)
    {
        try
        {
            var outbreak = await _outbreakService.GetOutbreakDetailsAsync(outbreakId);
            if (outbreak == null)
            {
                return NotFound();
            }

            return Ok(outbreak);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving outbreak {OutbreakId}", outbreakId);
            return StatusCode(500, new { message = "An error occurred while retrieving outbreak details" });
        }
    }

    [HttpGet("location/{location}")]
    public async Task<ActionResult<List<OutbreakAlertResponse>>> GetOutbreaksByLocation(string location)
    {
        try
        {
            var outbreaks = await _outbreakService.GetOutbreaksByLocationAsync(location);
            return Ok(outbreaks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving outbreaks for location {Location}", location);
            return StatusCode(500, new { message = "An error occurred while retrieving outbreaks by location" });
        }
    }

    [HttpPost("symptoms")]
    public async Task<ActionResult<List<OutbreakAlertResponse>>> GetOutbreaksBySymptoms([FromBody] List<string> symptoms)
    {
        try
        {
            if (symptoms == null || !symptoms.Any())
            {
                return BadRequest(new { message = "Symptoms list cannot be empty" });
            }

            var outbreaks = await _outbreakService.GetOutbreaksBySymptomsAsync(symptoms);
            return Ok(outbreaks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving outbreaks by symptoms");
            return StatusCode(500, new { message = "An error occurred while retrieving outbreaks by symptoms" });
        }
    }
}