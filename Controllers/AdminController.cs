using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Admin;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

/// <summary>
/// Admin management endpoints for administrative tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly IDoctorRegistrationService _doctorRegistrationService;
    private readonly UserManager<User> _userManager;

    public AdminController(
        ApplicationDbContext context,
        ILogger<AdminController> logger,
        IDoctorRegistrationService doctorRegistrationService,
        UserManager<User> userManager)
    {
        _context = context;
        _logger = logger;
        _doctorRegistrationService = doctorRegistrationService;
        _userManager = userManager;
    }

    /// <summary>
    /// Get doctors pending verification (Admin only)
    /// </summary>
    [HttpGet("doctors/pending")]
    public async Task<ActionResult<List<DoctorVerificationDto>>> GetPendingDoctorVerifications()
    {
        try
        {
            var pendingDoctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => !d.IsVerified)
                .Select(d => new DoctorVerificationDto
                {
                    Id = d.Id,
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Email = d.User.Email,
                    Specialization = d.Specialization,
                    LicenseNumber = d.LicenseNumber,
                    PhoneNumber = d.PhoneNumber,
                    Address = d.Address,
                    City = d.City,
                    Province = d.Province,
                    PostalCode = d.PostalCode,
                    YearsOfExperience = d.YearsOfExperience,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(pendingDoctors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending doctor verifications");
            return StatusCode(500, new { message = "An error occurred while retrieving pending verifications" });
        }
    }

    /// <summary>
    /// Verify/Approve or Reject a doctor registration (Admin only)
    /// </summary>
    [HttpPost("doctors/{id}/verify")]
    public async Task<ActionResult> VerifyDoctor(int id, [FromBody] VerifyDoctorRequest request)
    {
        try
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound("Doctor not found");
            }

            if (doctor.IsVerified)
            {
                return BadRequest("Doctor is already verified");
            }

            doctor.IsVerified = request.IsApproved;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var action = request.IsApproved ? "approved" : "rejected";
            _logger.LogInformation("Doctor {DoctorId} {Action} by admin", id, action);

            return Ok(new { message = $"Doctor {action} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying doctor {DoctorId}", id);
            return StatusCode(500, new { message = "An error occurred while verifying the doctor" });
        }
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserSummaryDto>>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    CreatedAt = u.CreatedAt,
                    IsExternalAuth = u.IsExternalAuth,
                    //IsAdmin = u.IsAdmin,
                    IsDoctor = u.IsDoctor
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get system statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AdminStatisticsDto>> GetSystemStatistics()
    {
        try
        {
            var stats = new AdminStatisticsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalSymptomReports = await _context.SymptomReports.CountAsync(),
                ActiveOutbreaks = await _context.OutbreakAlerts.CountAsync(o => o.Status == OutbreakStatus.Active),
                RecentRegistrations = await _context.Users
                    .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync(),
                RecentAppointments = await _context.Appointments
                    .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Update doctor information (Admin only)
    /// </summary>
    [HttpPut("doctors/{id}")]
    public async Task<ActionResult<Doctor>> UpdateDoctor(int id, [FromBody] UpdateDoctorRequest request)
    {
        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            if (request.FirstName != null) doctor.User.FirstName = request.FirstName;
            if (request.LastName != null) doctor.User.LastName = request.LastName;
            if (request.Email != null) doctor.User.Email = request.Email;
            if (request.Specialization != null) doctor.Specialization = request.Specialization;
            if (request.PhoneNumber != null) doctor.PhoneNumber = request.PhoneNumber;
            if (request.Address != null) doctor.Address = request.Address;
            if (request.City != null) doctor.City = request.City;
            if (request.Province != null) doctor.Province = request.Province;
            if (request.PostalCode != null) doctor.PostalCode = request.PostalCode;
            if (request.YearsOfExperience.HasValue) doctor.YearsOfExperience = request.YearsOfExperience.Value;
            if (request.IsAvailable.HasValue) doctor.IsAvailable = request.IsAvailable.Value;
            if (request.Latitude.HasValue) doctor.Latitude = request.Latitude.Value;
            if (request.Longitude.HasValue) doctor.Longitude = request.Longitude.Value;

            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor {DoctorId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the doctor" });
        }
    }

    /// <summary>
    /// Delete doctor (Admin only)
    /// </summary>
    [HttpDelete("doctors/{id}")]
    public async Task<ActionResult> DeleteDoctor(int id)
    {
        try
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor {DoctorId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the doctor" });
        }
    }
}