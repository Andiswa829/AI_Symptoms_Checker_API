using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Doctor;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

/// <summary>
/// Doctor dashboard and management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DoctorDashboardController> _logger;
    private readonly IDoctorRegistrationService _doctorRegistrationService;

    public DoctorDashboardController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DoctorDashboardController> logger,
        IDoctorRegistrationService doctorRegistrationService)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _doctorRegistrationService = doctorRegistrationService;
    }

    /// <summary>
    /// Get doctor dashboard statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<DoctorDashboardStatsDto>> GetDoctorStatistics()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Find the doctor record
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == currentUser.Email);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }

            var today = DateTime.UtcNow.Date;
            var thisWeek = DateTime.UtcNow.AddDays(-7);
            var thisMonth = DateTime.UtcNow.AddDays(-30);

            var stats = new DoctorDashboardStatsDto
            {
                TotalAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id),
                TodaysAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today),
                ThisWeekAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate >= thisWeek),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Scheduled),
                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed),
                TotalPatients = await _context.Appointments
                    .Where(a => a.DoctorId == doctor.Id)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync(),
                Rating = doctor.Rating ?? 0,
                YearsOfExperience = doctor.YearsOfExperience
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get doctor's appointments
    /// </summary>
    [HttpGet("appointments")]
    public async Task<ActionResult<List<DoctorAppointmentDto>>> GetDoctorAppointments(
        [FromQuery] DateTime? date = null,
        [FromQuery] AppointmentStatus? status = null)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == currentUser.Email);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }

            var query = _context.Appointments
                .Include(a => a.User)
                .Where(a => a.DoctorId == doctor.Id);

            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new DoctorAppointmentDto
                {
                    Id = a.Id,
                    PatientName = $"{a.User.FirstName} {a.User.LastName}",
                    PatientEmail = a.User.Email ?? "",
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = a.Status,
                    Notes = a.Notes,
                    PatientId = a.UserId
                })
                .ToListAsync();

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor appointments");
            return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
        }
    }

    /// <summary>
    /// Update appointment status and notes
    /// </summary>
    [HttpPut("appointments/{appointmentId}")]
    public async Task<ActionResult> UpdateAppointment(int appointmentId, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == currentUser.Email);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound("Appointment not found");
            }

            appointment.Status = request.Status;
            appointment.Notes = request.Notes;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { message = "An error occurred while updating the appointment" });
        }
    }
}