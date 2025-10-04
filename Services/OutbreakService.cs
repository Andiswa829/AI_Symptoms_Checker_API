using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Outbreaks;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Services;

public class OutbreakService : IOutbreakService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OutbreakService> _logger;

    public OutbreakService(ApplicationDbContext context, ILogger<OutbreakService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<OutbreakAlertResponse>> GetActiveOutbreaksAsync(string? location = null)
    {
        var query = _context.OutbreakAlerts
            .Where(o => o.Status == OutbreakStatus.Active);

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(o => o.Location.Contains(location) || 
                                   o.AffectedArea != null && o.AffectedArea.Contains(location));
        }

        var outbreaks = await query
            .OrderByDescending(o => o.Severity)
            .ThenByDescending(o => o.CreatedAt)
            .ToListAsync();

        return outbreaks.Select(MapToResponse).ToList();
    }

    public async Task<OutbreakAlertResponse?> GetOutbreakDetailsAsync(int outbreakId)
    {
        var outbreak = await _context.OutbreakAlerts
            .FirstOrDefaultAsync(o => o.Id == outbreakId);

        return outbreak != null ? MapToResponse(outbreak) : null;
    }

    public async Task<List<OutbreakAlertResponse>> GetOutbreaksByLocationAsync(string location)
    {
        var outbreaks = await _context.OutbreakAlerts
            .Where(o => o.Location.Contains(location) || 
                       o.AffectedArea != null && o.AffectedArea.Contains(location))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return outbreaks.Select(MapToResponse).ToList();
    }

    public async Task<List<OutbreakAlertResponse>> GetOutbreaksBySymptomsAsync(List<string> symptoms)
    {
        // This is a simplified implementation. In production, you might want more sophisticated matching
        var outbreaks = await _context.OutbreakAlerts
            .Where(o => o.Status == OutbreakStatus.Active)
            .ToListAsync();

        var matchingOutbreaks = outbreaks
            .Where(o => o.Symptoms != null && o.Symptoms.Any(s => 
                symptoms.Any(userSymptom => s.ToLower().Contains(userSymptom.ToLower()))))
            .OrderByDescending(o => o.Severity)
            .Select(o => MapToResponse(o))
            .ToList();

        return matchingOutbreaks;
    }

    private static OutbreakAlertResponse MapToResponse(OutbreakAlert o)
    {
        return new OutbreakAlertResponse
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            Disease = o.Disease,
            Location = o.Location,
            AffectedArea = o.AffectedArea,
            CaseCount = o.CaseCount,
            Severity = o.Severity,
            Status = o.Status,
            Symptoms = o.Symptoms,
            PreventionMeasures = o.PreventionMeasures,
            Source = o.Source,
            SourceUrl = o.SourceUrl,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            CreatedAt = o.CreatedAt
        };
    }
}