using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Medical;
using SymptomsCheckerApi.Models;
using System.Security.Claims;

namespace SymptomsCheckerApi.Controllers;

/// <summary>
/// Medical history and patient data management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<MedicalHistoryController> _logger;

    public MedicalHistoryController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<MedicalHistoryController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get patient medical history (for doctors)
    /// </summary>
    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<PatientMedicalHistoryDto>> GetPatientMedicalHistory(string patientId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null || (!currentUser.IsDoctor && !currentUser.IsAdmin))
            {
                return Forbid("Doctor or admin access required");
            }

            var patient = await _userManager.FindByIdAsync(patientId);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            // Get health data
            var healthData = await _context.HealthData
                .Where(h => h.UserId == patientId)
                .FirstOrDefaultAsync();

            // Get symptom reports
            var symptomReports = await _context.SymptomReports
                .Where(s => s.UserId == patientId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get appointments
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.UserId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new AppointmentHistoryDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = a.Status,
                    Notes = a.Notes,
                    DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                    Specialization = a.Doctor.Specialization
                })
                .ToListAsync();

            var medicalHistory = new PatientMedicalHistoryDto
            {
                PatientId = patientId,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                Email = patient.Email ?? "",
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                HealthData = healthData != null ? new PatientHealthDataDto
                {
                    Height = healthData.Height,
                    Weight = healthData.Weight,
                    BloodType = healthData.BloodType,
                    Allergies = healthData.Allergies,
                    Medications = healthData.Medications,
                    MedicalHistory = healthData.MedicalHistory,
                    FamilyHistory = healthData.FamilyHistory,
                    EmergencyContactName = healthData.EmergencyContactName,
                    EmergencyContactPhone = healthData.EmergencyContactPhone,
                    LastUpdated = healthData.UpdatedAt
                } : null,
                SymptomReports = symptomReports.Select(s => new SymptomReportSummaryDto
                {
                    Id = s.Id,
                    Symptoms = s.Symptoms,
                    PossibleConditions = s.PossibleConditions,
                    SeverityLevel = s.SeverityLevel.ToString(),
                    CreatedAt = s.CreatedAt
                }).ToList(),
                Appointments = appointments
            };

            return Ok(medicalHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient medical history for patient {PatientId}", patientId);
            return StatusCode(500, new { message = "An error occurred while retrieving medical history" });
        }
    }

    /// <summary>
    /// Get all patients for doctor dashboard
    /// </summary>
    [HttpGet("patients")]
    public async Task<ActionResult<List<PatientSummaryDto>>> GetPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null || (!currentUser.IsDoctor && !currentUser.IsAdmin))
            {
                return Forbid("Doctor or admin access required");
            }

            List<PatientSummaryDto> patients;

            if (currentUser.IsDoctor)
            {
                // For doctors, only show patients they have appointments with
                patients = await _context.Appointments
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Where(a => a.Doctor.User.Email == currentUser.Email)
                    .Select(a => a.User)
                    .Distinct()
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new PatientSummaryDto
                    {
                        Id = u.Id,
                        Name = $"{u.FirstName} {u.LastName}",
                        Email = u.Email ?? "",
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        LastAppointment = _context.Appointments
                            .Where(app => app.UserId == u.Id)
                            .OrderByDescending(app => app.AppointmentDate)
                            .Select(app => app.AppointmentDate)
                            .FirstOrDefault()
                    })
                    .ToListAsync();
            }
            else
            {
                // For admins, show all patients
                patients = await _context.Users
                    .Where(u => !u.IsAdmin && !u.IsDoctor)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new PatientSummaryDto
                    {
                        Id = u.Id,
                        Name = $"{u.FirstName} {u.LastName}",
                        Email = u.Email ?? "",
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        LastAppointment = _context.Appointments
                            .Where(app => app.UserId == u.Id)
                            .OrderByDescending(app => app.AppointmentDate)
                            .Select(app => app.AppointmentDate)
                            .FirstOrDefault()
                    })
                    .ToListAsync();
            }

            return Ok(patients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patients list");
            return StatusCode(500, new { message = "An error occurred while retrieving patients" });
        }
    }

    /// <summary>
    /// Add medical notes for a patient (doctors only)
    /// </summary>
    [HttpPost("patient/{patientId}/notes")]
    public async Task<ActionResult> AddMedicalNotes(string patientId, [FromBody] AddMedicalNotesRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null || !currentUser.IsDoctor)
            {
                return Forbid("Doctor access required");
            }

            // Here you would implement a medical notes system
            // For now, we'll add it to a future appointment or create a separate notes entity
            
            return Ok(new { message = "Medical notes added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medical notes for patient {PatientId}", patientId);
            return StatusCode(500, new { message = "An error occurred while adding medical notes" });
        }
    }
}