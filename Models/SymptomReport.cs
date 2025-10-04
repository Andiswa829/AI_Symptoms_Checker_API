using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.Models;

public class SymptomReport
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public List<string> Symptoms { get; set; } = new();
    
    public string? Description { get; set; }
    public int? SeverityLevel { get; set; } // 1-10 scale
    public DateTime OnsetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // AI Analysis Results
    public string? AnalysisResult { get; set; }
    public List<string>? PossibleConditions { get; set; } = new();
    public string? RecommendedActions { get; set; }
    public bool RequiresImmediateAttention { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}