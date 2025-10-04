namespace SymptomsCheckerApi.DTOs.Health;

public class HealthDataRequest
{
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
}