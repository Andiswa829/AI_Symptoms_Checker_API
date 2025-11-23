namespace SymptomsCheckerApi.DTOs.Admin;

public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsExternalAuth { get; set; }
    public bool IsDoctor { get; set; }
}

public class AdminStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int TotalSymptomReports { get; set; }
    public int ActiveOutbreaks { get; set; }
    public int RecentRegistrations { get; set; }
    public int RecentAppointments { get; set; }
}

public class UpdateDoctorRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool? IsAvailable { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}