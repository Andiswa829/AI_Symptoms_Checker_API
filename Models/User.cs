using Microsoft.AspNetCore.Identity;
using System.Data;

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
    
    // Admin and Role Management
    public bool IsAdmin { get; set; } = false;
    public bool IsDoctor { get; set; } = false;
    
    // Medical History Access
    public bool CanViewMedicalHistory { get; set; } = false;
    
    // Health Data Consent
    public bool HasHealthDataConsent { get; set; } = false;
    public DateTime? HealthDataConsentDate { get; set; }
    public string? GoogleHealthAccessToken { get; set; }
    public DateTime? GoogleHealthTokenExpiry { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Role navigation property
    //public int RoleId { get; set; }
    //public Role Role { get; set; } = null!;

    // Optional: Link to Doctor if this user is a doctor
    // public Doctor? DoctorProfile { get; set; }

    // Navigation properties
    public ICollection<SymptomReport> SymptomReports { get; set; } = new List<SymptomReport>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<HealthData> HealthData { get; set; } = new List<HealthData>();
}