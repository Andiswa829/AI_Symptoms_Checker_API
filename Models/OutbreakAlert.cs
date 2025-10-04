using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.Models;

public class OutbreakAlert
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Disease { get; set; } = string.Empty;
    
    [Required]
    public string Location { get; set; } = string.Empty;
    
    public string? AffectedArea { get; set; }
    public int? CaseCount { get; set; }
    public OutbreakSeverity Severity { get; set; } = OutbreakSeverity.Low;
    public OutbreakStatus Status { get; set; } = OutbreakStatus.Active;
    
    public List<string>? Symptoms { get; set; } = new();
    public List<string>? PreventionMeasures { get; set; } = new();
    
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum OutbreakSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum OutbreakStatus
{
    Active,
    Monitoring,
    Resolved,
    Archived
}