using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Appointments;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SymptomsCheckerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;
    private readonly ApplicationDbContext _context;

    public AppointmentsController(
        IAppointmentService appointmentService,
        ILogger<AppointmentsController> logger,
        ApplicationDbContext context)
    {
        _appointmentService = appointmentService;
        _logger = logger;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> BookAppointment([FromBody] AppointmentRequest request)
    {
        try
        {
            var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Allow admins to specify a different userId in the request
            var isAdmin = User.IsInRole("Admin");
            string finalUserId;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                if (!isAdmin && request.UserId != requestingUserId)
                {
                    return Forbid();
                }

                finalUserId = request.UserId!;
            }
            else
            {
                finalUserId = requestingUserId!;
            }

            var result = await _appointmentService.BookAppointmentAsync(request, finalUserId);
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

    [HttpGet("getUserAppointments/{userId}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAppointments(string userId)
    {
        try
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }

    [HttpGet("getAppointment{appointmentId}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointment(int appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

    [HttpDelete("cancelAppointment/{appointmentId}")]
    public async Task<ActionResult> CancelAppointment(int appointmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

    [HttpGet("{doctorId}/availableSlots")]
    public async Task<ActionResult<List<DateTime>>> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date = null)
    {
        try
        {
            var requestedDate = date?.Date ?? DateTime.UtcNow.Date;
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, requestedDate);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available slots for doctor {DoctorId}", doctorId);
            return StatusCode(500, new { message = "An error occurred while retrieving available slots" });
        }
    }

    [HttpGet("{doctorId}/availability")]
    public async Task<ActionResult<List<DoctorAvailability>>> GetDoctorAvailability(int doctorId, DateTime date)
    {
        try
        {
            var requestedDate = date != null?date: DateTime.UtcNow.Date;
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, requestedDate);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }
}