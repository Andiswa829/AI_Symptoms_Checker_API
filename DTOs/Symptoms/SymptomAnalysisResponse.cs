namespace SymptomsCheckerApi.DTOs.Symptoms;

public class SymptomAnalysisResponse
{
    public int ReportId { get; set; }
    public List<string> Symptoms { get; set; } = new();
    public string? Description { get; set; }
    public int? SeverityLevel { get; set; }
    public DateTime OnsetDate { get; set; }
    
    // AI Analysis Results
    public string AnalysisResult { get; set; } = string.Empty;
    public List<PossibleCondition> PossibleConditions { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public bool RequiresImmediateAttention { get; set; }
    public string Disclaimer { get; set; } = "This analysis is for informational purposes only and should not replace professional medical advice.";
    
    public DateTime CreatedAt { get; set; }
}

public class PossibleCondition
{
    public string Name { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> MatchingSymptoms { get; set; } = new();
}