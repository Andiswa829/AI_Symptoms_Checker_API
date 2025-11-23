using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.DTOs.Medical;

public class PatientMedicalHistoryDto
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public PatientHealthDataDto? HealthData { get; set; }
    public List<SymptomReportSummaryDto> SymptomReports { get; set; } = new();
    public List<AppointmentHistoryDto> Appointments { get; set; } = new();
}

public class PatientHealthDataDto
{
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? BloodType { get; set; }
    public List<string>? Allergies { get; set; } = new();
    public List<string>? Medications { get; set; } = new();
    public List<string>? MedicalHistory { get; set; } = new();
    public List<string>? FamilyHistory { get; set; } = new();
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class SymptomReportSummaryDto
{
    public int Id { get; set; }
    public List<string> Symptoms { get; set; } = new();
    public List<string> PossibleConditions { get; set; } = new();
    public string? SeverityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentHistoryDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}

public class PatientSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateTime? LastAppointment { get; set; }
}

public class AddMedicalNotesRequest
{
    public string Notes { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
}