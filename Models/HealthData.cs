using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.Models;

public class HealthData
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public double? Height { get; set; } // in cm
    public double? Weight { get; set; } // in kg
    public string? BloodType { get; set; }
    public List<string>? Allergies { get; set; } = new();
    public List<string>? Medications { get; set; } = new();
    public List<string>? MedicalHistory { get; set; } = new();
    public List<string>? FamilyHistory { get; set; } = new();
    
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
}