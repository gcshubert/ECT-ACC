using ECT.ACC.Contracts.DTOs;
using ECT.ACC.Data.Context;
using ECT.ACC.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/ProcessDomains")]
public class ProcessDomainsController : ControllerBase
{
    private readonly ECTDbContext _db;
    public ProcessDomainsController(ECTDbContext db) => _db = db;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProcessDomainDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var domains = await _db.ProcessDomains
            .Include(d => d.Templates)
                .ThenInclude(t => t.ParameterDefinitions)
            .AsNoTracking()
            .ToListAsync();

        return Ok(domains.Select(MapDomain).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProcessDomainDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var d = await _db.ProcessDomains
            .Include(d => d.Templates)
                .ThenInclude(t => t.ParameterDefinitions)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return d is null ? NotFound() : Ok(MapDomain(d));
    }

    private static ProcessDomainDto MapDomain(ProcessDomain d) => new(
        d.Id, d.Name, d.Description, d.IconKey,
        d.Templates.Select(t => new ParameterTemplateSummaryDto(
            t.Id, t.Name, t.Description,
            t.ParameterDefinitions.OrderBy(p => p.SortOrder).Select(p =>
                new TemplateParameterDefinitionDto(
                    p.Id, p.Key, p.Symbol, p.Label, p.Description,
                    p.DefaultUnit, p.SortOrder, p.IsEctCoreParameter,
                    p.SeedValue is null ? null
    : new ScientificValueDto { Coefficient = p.SeedValue.Coefficient, Exponent = p.SeedValue.Exponent }
                )).ToList()
        )).ToList()
    );
}
