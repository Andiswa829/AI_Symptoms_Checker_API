using SymptomsCheckerApi.DTOs.Outbreaks;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IOutbreakService
{
    Task<List<OutbreakAlertResponse>> GetActiveOutbreaksAsync(string? location = null);
    Task<OutbreakAlertResponse?> GetOutbreakDetailsAsync(int outbreakId);
    Task<List<OutbreakAlertResponse>> GetOutbreaksByLocationAsync(string location);
    Task<List<OutbreakAlertResponse>> GetOutbreaksBySymptomsAsync(List<string> symptoms);
}