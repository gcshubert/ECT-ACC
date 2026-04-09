using Microsoft.EntityFrameworkCore;
using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using ECT.ACC.Data.Math;

namespace ECT.ACC.Api.Services;

public interface IScenarioConfigurationService
{
    Task<List<ScenarioConfigurationDto>> GetConfigurationsAsync(int scenarioId);
    Task<ScenarioConfigurationDto?>      GetConfigurationAsync(int scenarioId, int configId);
    Task<ScenarioConfigurationDto>       CreateConfigurationAsync(int scenarioId, CreateScenarioConfigurationRequest request);
    Task<ScenarioConfigurationDto?>      UpdateConfigurationAsync(int scenarioId, int configId, UpdateScenarioConfigurationRequest request);
    Task<ScenarioConfigurationDto?>      UpdateEntryAsync(int scenarioId, int configId, string paramKey, UpdateConfigurationEntryRequest request);
    Task<ScenarioConfigurationDto?>      ActivateConfigurationAsync(int scenarioId, int configId, IDeficitAnalysisService deficitService);
    Task<DeficitAnalysisDto?>            ComputeDeficitAnalysisAsync(int scenarioId, int configId, IDeficitAnalysisService deficitService);
    Task                                EnsureDefaultConfigurationAsync(Scenario scenario, IDeficitAnalysisService deficitService);
    Task<bool>                           DeleteConfigurationAsync(int scenarioId, int configId);
    Task                                WriteHierarchicalSnapshotAsync(int scenarioId, int configId, GraphWalkResultTree walkResult);
    Task<DeficitAnalysisDto?>            ComputeHierarchicalDeficitAnalysisAsync(int scenarioId, int configId, GraphWalkResultTree rollupResult, IDeficitAnalysisService deficitService);
}

public class ScenarioConfigurationService : IScenarioConfigurationService
{
    private readonly ECTDbContext _db;

    public ScenarioConfigurationService(ECTDbContext db)
    {
        _db = db;
    }

    // ── GET all ──────────────────────────────────────────────────────────────

    public async Task<List<ScenarioConfigurationDto>> GetConfigurationsAsync(int scenarioId)
    {
        var configs = await _db.ScenarioConfigurations
            .Where(c => c.ScenarioId == scenarioId)
            .Include(c => c.Entries)
            .Include(c => c.DeficitAnalysis)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CreatedDate)
            .ToListAsync();

        return configs.Select(MapToDto).ToList();
    }

    // ── GET one ──────────────────────────────────────────────────────────────

    public async Task<ScenarioConfigurationDto?> GetConfigurationAsync(int scenarioId, int configId)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        return config is null ? null : MapToDto(config);
    }

    // ── CREATE (clone from another config or from current active state) ──────

    public async Task<ScenarioConfigurationDto> CreateConfigurationAsync(
        int scenarioId,
        CreateScenarioConfigurationRequest request)
    {
        List<ScenarioConfigurationEntry> sourceEntries;

        if (request.CloneFromConfigurationId.HasValue)
        {
            // Clone entries from a named configuration
            var source = await LoadConfigAsync(scenarioId, request.CloneFromConfigurationId.Value)
                         ?? throw new InvalidOperationException(
                                $"Source configuration {request.CloneFromConfigurationId} not found.");
            sourceEntries = source.Entries.ToList();
        }
        else
        {
            // Clone from current active state:
            // For each parameter that has documentation, read the active variant (if any).
            sourceEntries = await BuildEntriesFromActiveStateAsync(scenarioId);
        }

        var nextSort = await _db.ScenarioConfigurations
            .Where(c => c.ScenarioId == scenarioId)
            .MaxAsync(c => (int?)c.SortOrder) ?? -1;

        var config = new ScenarioConfiguration
        {
            ScenarioId  = scenarioId,
            Name        = request.Name,
            Description = request.Description,
            SortOrder   = nextSort + 1,
            CreatedDate = DateTime.UtcNow,
            Entries     = sourceEntries.Select(e => new ScenarioConfigurationEntry
            {
                ParameterKey  = e.ParameterKey,
                VariantId     = e.VariantId,
                VariantLabel  = e.VariantLabel,
                SnapshotValue = null, // populated on Activate
            }).ToList(),
        };

        _db.ScenarioConfigurations.Add(config);
        await _db.SaveChangesAsync();

        return MapToDto(config);
    }

    public async Task EnsureDefaultConfigurationAsync(
    Scenario scenario,
    IDeficitAnalysisService deficitService)
    {
        // Flat → "Base"
        // Hierarchical → "H - Rollup"
        var configName = scenario.ScenarioMode == "Hierarchical"
            ? "H - Rollup"
            : "Base";

        // If it already exists, nothing to do
        var exists = await _db.ScenarioConfigurations
            .AnyAsync(c => c.ScenarioId == scenario.Id && c.Name == configName);

        if (exists) return;

        // Create a new configuration using the existing creation pipeline
        var created = await CreateConfigurationAsync(
            scenario.Id,
            new CreateScenarioConfigurationRequest
            {
                Name = configName,
                Description = string.Empty
            });

        // Activate it so snapshot values are computed (deficit analysis handled separately)
        await ActivateConfigurationAsync(
            scenario.Id,
            created.Id,
            deficitService);
    }


    // ── UPDATE name / description / sortOrder ────────────────────────────────

    public async Task<ScenarioConfigurationDto?> UpdateConfigurationAsync(
        int scenarioId, int configId, UpdateScenarioConfigurationRequest request)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        if (config is null) return null;

        config.Name        = request.Name;
        config.Description = request.Description;
        config.SortOrder   = request.SortOrder;

        await _db.SaveChangesAsync();
        return MapToDto(config);
    }

    // ── UPDATE single entry (swap variant for one parameter) ─────────────────

    public async Task<ScenarioConfigurationDto?> UpdateEntryAsync(
        int scenarioId, int configId, string paramKey, UpdateConfigurationEntryRequest request)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        if (config is null) return null;

        var entry = config.Entries.FirstOrDefault(
            e => string.Equals(e.ParameterKey, paramKey, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            // Entry doesn't exist yet — create it
            entry = new ScenarioConfigurationEntry
            {
                ConfigurationId = configId,
                ParameterKey    = paramKey,
            };
            config.Entries.Add(entry);
        }

        entry.VariantId    = request.VariantId;
        entry.VariantLabel = await ResolveVariantLabelAsync(scenarioId, paramKey, request.VariantId);
        entry.SnapshotValue = null; // cleared — will repopulate on next Activate

        await _db.SaveChangesAsync();
        return MapToDto(config);
    }

    // ── ACTIVATE (apply all entries + recompute deficit) ─────────────────────

    public async Task<ScenarioConfigurationDto?> ActivateConfigurationAsync(
        int scenarioId, int configId, IDeficitAnalysisService deficitService)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        if (config is null) return null;

        // 1. For each entry, activate the chosen variant (or base) on the parameter
        foreach (var entry in config.Entries)
        {
            var doc = await _db.ParameterDocumentations
                .Include(d => d.Variants)
                    .ThenInclude(v => v.SubParameters)
                .Include(d => d.SubParameters)
                .FirstOrDefaultAsync(d => d.ScenarioId == scenarioId
                                       && d.ParameterKey == entry.ParameterKey);

            if (doc is null) continue;

            // Deactivate all variants first
            foreach (var v in doc.Variants)
                v.IsActive = false;

            // Activate chosen variant (if any)
            if (entry.VariantId.HasValue)
            {
                var target = doc.Variants.FirstOrDefault(v => v.Id == entry.VariantId.Value);
                if (target is not null)
                {
                    target.IsActive = true;
                    entry.VariantLabel = target.Name;
                }
            }
            else
            {
                entry.VariantLabel = "Base";
            }

            // 2. Snapshot the composed value via rollup
            var activeSteps = entry.VariantId.HasValue
                ? doc.Variants
                      .FirstOrDefault(v => v.Id == entry.VariantId.Value)
                      ?.SubParameters
                      .OrderBy(s => s.StepOrder)
                      .Select(s => (s.Value.Coefficient, (int)s.Value.Exponent, s.Operation))
                  ?? Enumerable.Empty<(double, int, StepOperation)>()
                : doc.SubParameters
                      .OrderBy(s => s.StepOrder)
                      .Select(s => (s.Value.Coefficient, (int)s.Value.Exponent, s.Operation));

            var rollup = DerivationRollupService.Compute(activeSteps);
            entry.SnapshotValue = new ScientificValueOwned
            {
                Coefficient = rollup.Coefficient,
                Exponent = rollup.Exponent,
            };
        }

        await _db.SaveChangesAsync();

        // 3. Reload to get fresh navigation properties
        var refreshed = await LoadConfigAsync(scenarioId, configId);
        return refreshed is null ? null : MapToDto(refreshed);
    }

    public async Task WriteHierarchicalSnapshotAsync(
        int scenarioId, int configId, GraphWalkResultTree walkResult)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        if (config is null)
            throw new InvalidOperationException(
                $"Configuration {configId} not found for scenario {scenarioId}");

        // Create entries without snapshot values first
        var parameters = new[] { "e", "c", "k", "t" };
        var values = new[] { walkResult.Energy, walkResult.Control, walkResult.Complexity, walkResult.TimeAvailable };

        for (int i = 0; i < parameters.Length; i++)
        {
            var key = parameters[i];
            var value = values[i];
            
            if (value is null) continue;
            
            // Safety check for valid values
            if (double.IsNaN(value.Coefficient) || double.IsInfinity(value.Coefficient) ||
                double.IsNaN(value.Exponent) || double.IsInfinity(value.Exponent))
            {
                continue;
            }

            var entry = config.Entries.FirstOrDefault(
                e => string.Equals(e.ParameterKey, key, StringComparison.OrdinalIgnoreCase));
                
            if (entry is null)
            {
                entry = new ScenarioConfigurationEntry
                {
                    ConfigurationId = configId,
                    ParameterKey = key,
                    VariantId = null,
                    VariantLabel = "Hierarchical Rollup"
                };
                config.Entries.Add(entry);
            }

            // Set snapshot value
            entry.SnapshotValue = new ScientificValueOwned
            {
                Coefficient = value.Coefficient,
                Exponent = value.Exponent,
            };
        }

        await _db.SaveChangesAsync();
    }

    public async Task<DeficitAnalysisDto?> ComputeHierarchicalDeficitAnalysisAsync(
        int scenarioId, int configId, GraphWalkResultTree rollupResult, IDeficitAnalysisService deficitService)
    {
        // Use the rollup result values directly instead of reading from database
        var energy = rollupResult.Energy;
        var control = rollupResult.Control;
        var complexity = rollupResult.Complexity;
        var timeAvailable = rollupResult.TimeAvailable;

        // Compute deficit from rollup values, tied directly to this configuration
        await deficitService.ComputeAndSaveFromRollupAsync(
            scenarioId, configId, energy, control, complexity, timeAvailable);

        // Reload to get fresh navigation properties including deficit analysis
        var refreshed = await LoadConfigAsync(scenarioId, configId);
        return refreshed?.DeficitAnalysis is null ? null : MapDeficit(refreshed.DeficitAnalysis);
    }

    // ---- COMPUTE DEFICIT ANALYSIS ----

    public async Task<DeficitAnalysisDto?> ComputeDeficitAnalysisAsync(
        int scenarioId, int configId, IDeficitAnalysisService deficitService)
    {
        var config = await LoadConfigAsync(scenarioId, configId);
        if (config is null) return null;

        // Extract the four core ECT parameter snapshot values
        ScientificValue Snap(string key)
        {
            var entry = config.Entries.FirstOrDefault(
                e => string.Equals(e.ParameterKey, key, StringComparison.OrdinalIgnoreCase));
            if (entry?.SnapshotValue is null)
                throw new InvalidOperationException(
                    $"Core parameter '{key}' has no snapshot value after rollup. " +
                    $"Ensure ParameterDocumentation and at least a base sub-parameter exist for this key.");
            return new ScientificValue(
                entry.SnapshotValue.Coefficient,
                entry.SnapshotValue.Exponent);
        }

        var energy       = Snap("e");
        var control      = Snap("c");
        var complexity   = Snap("k");
        var timeAvailable = Snap("t");

        // Compute deficit from rollup values, tied directly to this configuration
        await deficitService.ComputeAndSaveFromRollupAsync(
            scenarioId, configId, energy, control, complexity, timeAvailable);

        // Reload to get fresh navigation properties including deficit analysis
        var refreshed = await LoadConfigAsync(scenarioId, configId);
        return refreshed?.DeficitAnalysis is null ? null : MapDeficit(refreshed.DeficitAnalysis);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task<bool> DeleteConfigurationAsync(int scenarioId, int configId)
    {
        var config = await _db.ScenarioConfigurations
            .Include(c => c.DeficitAnalysis)
            .FirstOrDefaultAsync(c => c.Id == configId && c.ScenarioId == scenarioId);

        if (config is null) return false;

        if (config.DeficitAnalysis is not null)
            _db.DeficitAnalyses.Remove(config.DeficitAnalysis);

        _db.ScenarioConfigurations.Remove(config);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<ScenarioConfiguration?> LoadConfigAsync(int scenarioId, int configId)
        => await _db.ScenarioConfigurations
            .Where(c => c.ScenarioId == scenarioId && c.Id == configId)
            .Include(c => c.Entries)
            .Include(c => c.DeficitAnalysis)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Reads the current active state of the scenario — one entry per documented parameter.
    /// Active variant (if any) becomes the entry; no active variant → Base entry.
    /// </summary>
    private async Task<List<ScenarioConfigurationEntry>> BuildEntriesFromActiveStateAsync(int scenarioId)
    {
        var docs = await _db.ParameterDocumentations
            .Where(d => d.ScenarioId == scenarioId)
            .Include(d => d.Variants)
            .ToListAsync();

        return docs.Select(doc =>
        {
            var active = doc.Variants.FirstOrDefault(v => v.IsActive);
            return new ScenarioConfigurationEntry
            {
                ParameterKey  = doc.ParameterKey,
                VariantId     = active?.Id,
                VariantLabel  = active?.Name ?? "Base",
                SnapshotValue = null,
            };
        }).ToList();
    }

    private async Task<string> ResolveVariantLabelAsync(
        int scenarioId, string paramKey, int? variantId)
    {
        if (variantId is null) return "Base";

        var variant = await _db.ParameterVariants
            .Include(v => v.ParameterDocumentation)
            .FirstOrDefaultAsync(v => v.Id == variantId
                && v.ParameterDocumentation.ScenarioId == scenarioId
                && v.ParameterDocumentation.ParameterKey == paramKey);

        return variant?.Name ?? "Base";
    }

    // ── DTO mapping ──────────────────────────────────────────────────────────

    private static ScenarioConfigurationDto MapToDto(ScenarioConfiguration c) => new()
    {
        Id          = c.Id,
        ScenarioId  = c.ScenarioId,
        Name        = c.Name,
        Description = c.Description,
        SortOrder   = c.SortOrder,
        CreatedDate = c.CreatedDate.ToString("O"),
        Entries     = c.Entries.Select(MapEntryToDto).ToList(),
        DeficitAnalysis = c.DeficitAnalysis is null ? null : MapDeficit(c.DeficitAnalysis),
    };

    private static ScenarioConfigurationEntryDto MapEntryToDto(ScenarioConfigurationEntry e) => new()
    {
        Id            = e.Id,
        ParameterKey  = e.ParameterKey,
        VariantId     = e.VariantId,
        VariantLabel  = e.VariantLabel,
        SnapshotValue = e.SnapshotValue is null ? null : new ScientificValueDto
        {
            Coefficient = e.SnapshotValue.Coefficient,
            Exponent    = e.SnapshotValue.Exponent,
        },
    };

    private static DeficitAnalysisDto MapDeficit(DeficitAnalysis d) => new()
    {
        Id = d.Id,
        ScenarioId = d.ScenarioId,
        DeficitType = d.DeficitType,
        ClassificationNotes = d.ClassificationNotes,
        CRequired = new ScientificValueDto { Coefficient = d.CRequired.Coefficient, Exponent = d.CRequired.Exponent },
        CAvailable = new ScientificValueDto { Coefficient = d.CAvailable.Coefficient, Exponent = d.CAvailable.Exponent },
        CDeficit = new ScientificValueDto { Coefficient = d.CDeficit.Coefficient, Exponent = d.CDeficit.Exponent },
    };
}
