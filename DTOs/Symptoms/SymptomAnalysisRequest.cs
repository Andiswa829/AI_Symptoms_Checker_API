using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Symptoms;

public class SymptomAnalysisRequest
{
    [Required]
    public List<string> Symptoms { get; set; } = new();
    
    public string? Description { get; set; }
    
    [Range(1, 10)]
    public int? SeverityLevel { get; set; }
    
    public DateTime? OnsetDate { get; set; }
    
    // Additional context for AI analysis
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public List<string>? MedicalHistory { get; set; } = new();
    public List<string>? CurrentMedications { get; set; } = new();
}