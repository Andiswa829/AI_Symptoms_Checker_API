using Microsoft.AspNetCore.Identity;

namespace SymptomsCheckerApi.Models;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePicture { get; set; }
    public bool IsExternalAuth { get; set; } = false;
    public string? ExternalProvider { get; set; }
    public string? ExternalProviderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<SymptomReport> SymptomReports { get; set; } = new List<SymptomReport>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<HealthData> HealthData { get; set; } = new List<HealthData>();
}