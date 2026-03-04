using ECT.ACC.Api.Services;
using ECT.ACC.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECT.ACC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScenariosController : ControllerBase
{
    private readonly IScenarioService _scenarioService;

    public ScenariosController(IScenarioService scenarioService)
    {
        _scenarioService = scenarioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScenarioDto>>> GetAll()
    {
        var scenarios = await _scenarioService.GetAllAsync();
        return Ok(scenarios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScenarioDto>> GetById(int id)
    {
        var scenario = await _scenarioService.GetByIdAsync(id);
        return scenario is null ? NotFound() : Ok(scenario);
    }

    [HttpPost]
    public async Task<ActionResult<ScenarioDto>> Create(CreateScenarioDto dto)
    {
        var created = await _scenarioService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScenarioDto>> Update(int id, UpdateScenarioDto dto)
    {
        var updated = await _scenarioService.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _scenarioService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
