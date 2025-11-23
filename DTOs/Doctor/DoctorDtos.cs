using SymptomsCheckerApi.Models;
using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Doctor;

public class DoctorDashboardStatsDto
{
    public int TotalAppointments { get; set; }
    public int TodaysAppointments { get; set; }
    public int ThisWeekAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int TotalPatients { get; set; }
    public decimal Rating { get; set; }
    public int YearsOfExperience { get; set; }
}

public class DoctorAppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    [Required]
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class DoctorRegistrationRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string Specialization { get; set; } = string.Empty;
    
    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    public string Gender { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Range(0, 50)]
    public int YearsOfExperience { get; set; }
}