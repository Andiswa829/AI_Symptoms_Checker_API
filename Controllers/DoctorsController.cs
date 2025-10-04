using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(
        IAppointmentService appointmentService,
        ILogger<DoctorsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Doctor>>> SearchDoctors(
        [FromQuery] string? specialization = null,
        [FromQuery] string? location = null)
    {
        try
        {
            var doctors = await _appointmentService.SearchDoctorsAsync(specialization, location);
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors");
            return StatusCode(500, new { message = "An error occurred while searching for doctors" });
        }
    }
}