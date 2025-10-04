using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.DTOs.Appointments;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> BookAppointment([FromBody] AppointmentRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _appointmentService.BookAppointmentAsync(request, userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment");
            return StatusCode(500, new { message = "An error occurred while booking the appointment" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAppointments()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }

    [HttpGet("{appointmentId}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(int appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var appointment = await _appointmentService.GetAppointmentAsync(appointmentId, userId);
            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { message = "An error occurred while retrieving the appointment" });
        }
    }

    [HttpDelete("{appointmentId}")]
    public async Task<ActionResult> CancelAppointment(int appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { message = "An error occurred while cancelling the appointment" });
        }
    }
}