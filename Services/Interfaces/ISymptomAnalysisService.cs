using SymptomsCheckerApi.DTOs.Symptoms;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface ISymptomAnalysisService
{
    Task<SymptomAnalysisResponse> AnalyzeSymptomsAsync(SymptomAnalysisRequest request, string userId);
    Task<List<SymptomReport>> GetUserSymptomHistoryAsync(string userId);
    Task<SymptomReport?> GetSymptomReportAsync(int reportId, string userId);
}