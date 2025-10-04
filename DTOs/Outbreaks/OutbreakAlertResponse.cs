using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.DTOs.Outbreaks;

public class OutbreakAlertResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Disease { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AffectedArea { get; set; }
    public int? CaseCount { get; set; }
    public OutbreakSeverity Severity { get; set; }
    public OutbreakStatus Status { get; set; }
    
    public List<string>? Symptoms { get; set; } = new();
    public List<string>? PreventionMeasures { get; set; } = new();
    
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}