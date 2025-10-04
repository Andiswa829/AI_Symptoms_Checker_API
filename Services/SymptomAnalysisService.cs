using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Symptoms;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Services;

public class SymptomAnalysisService : ISymptomAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SymptomAnalysisService> _logger;

    public SymptomAnalysisService(ApplicationDbContext context, ILogger<SymptomAnalysisService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SymptomAnalysisResponse> AnalyzeSymptomsAsync(SymptomAnalysisRequest request, string userId)
    {
        try
        {
            // Create symptom report
            var symptomReport = new SymptomReport
            {
                UserId = userId,
                Symptoms = request.Symptoms,
                Description = request.Description,
                SeverityLevel = request.SeverityLevel,
                OnsetDate = request.OnsetDate ?? DateTime.UtcNow
            };

            // Perform AI analysis (simplified version - in production, integrate with AI service)
            var analysisResult = await PerformSymptomAnalysis(request);
            
            symptomReport.AnalysisResult = analysisResult.AnalysisResult;
            symptomReport.PossibleConditions = analysisResult.PossibleConditions.Select(c => c.Name).ToList();
            symptomReport.RecommendedActions = string.Join("; ", analysisResult.RecommendedActions);
            symptomReport.RequiresImmediateAttention = analysisResult.RequiresImmediateAttention;

            _context.SymptomReports.Add(symptomReport);
            await _context.SaveChangesAsync();

            return new SymptomAnalysisResponse
            {
                ReportId = symptomReport.Id,
                Symptoms = symptomReport.Symptoms,
                Description = symptomReport.Description,
                SeverityLevel = symptomReport.SeverityLevel,
                OnsetDate = symptomReport.OnsetDate,
                AnalysisResult = symptomReport.AnalysisResult,
                PossibleConditions = analysisResult.PossibleConditions,
                RecommendedActions = analysisResult.RecommendedActions,
                RequiresImmediateAttention = symptomReport.RequiresImmediateAttention,
                CreatedAt = symptomReport.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing symptoms for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<SymptomReport>> GetUserSymptomHistoryAsync(string userId)
    {
        return await _context.SymptomReports
            .Where(sr => sr.UserId == userId)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();
    }

    public async Task<SymptomReport?> GetSymptomReportAsync(int reportId, string userId)
    {
        return await _context.SymptomReports
            .FirstOrDefaultAsync(sr => sr.Id == reportId && sr.UserId == userId);
    }

    private async Task<SymptomAnalysisResponse> PerformSymptomAnalysis(SymptomAnalysisRequest request)
    {
        // This is a simplified analysis. In production, integrate with medical AI services
        var response = new SymptomAnalysisResponse();
        
        // Simple rule-based analysis
        var emergencySymptoms = new[] { "chest pain", "difficulty breathing", "severe headache", "loss of consciousness" };
        response.RequiresImmediateAttention = request.Symptoms.Any(s => 
            emergencySymptoms.Any(es => s.ToLower().Contains(es)));

        response.AnalysisResult = "Based on the symptoms provided, here are some possible conditions. Please consult with a healthcare professional for proper diagnosis.";
        
        // Mock conditions based on common symptoms
        if (request.Symptoms.Any(s => s.ToLower().Contains("fever")))
        {
            response.PossibleConditions.Add(new PossibleCondition
            {
                Name = "Viral Infection",
                Probability = 0.7,
                Description = "Common viral infection with fever",
                MatchingSymptoms = request.Symptoms.Where(s => s.ToLower().Contains("fever")).ToList()
            });
        }

        response.RecommendedActions = response.RequiresImmediateAttention 
            ? new List<string> { "Seek immediate medical attention", "Go to emergency room" }
            : new List<string> { "Monitor symptoms", "Rest and hydration", "Consult healthcare provider if symptoms worsen" };

        return await Task.FromResult(response);
    }
}