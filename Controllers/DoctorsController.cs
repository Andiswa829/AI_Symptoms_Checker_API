using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Appointments;
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
    public async Task<ActionResult<List<DoctorDto>>> SearchDoctors(
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

    /// <summary>
    /// Get all doctors (for admin use)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> GetAllDoctors()
    {
        try
        {
            var doctors = await _appointmentService.SearchDoctorsAsync();
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all doctors");
            return StatusCode(500, new { message = "An error occurred while retrieving doctors" });
        }
    }

    /// <summary>
    /// Get doctor by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorDto>> GetDoctorById(int id)
    {
        try
        {
            var doctor = await _appointmentService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound();
                
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor {DoctorId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving doctor" });
        }
    }
}