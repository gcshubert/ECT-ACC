using ECT.ACC.Contracts.DTOs;
namespace ECT.ACC.Api.Services;

public interface IScenarioService
{
    Task<IEnumerable<ScenarioDto>> GetAllAsync();
    Task<ScenarioDto?> GetByIdAsync(int id);
    Task<ScenarioDto> CreateAsync(CreateScenarioDto dto);
    Task<ScenarioDto?> UpdateAsync(int id, UpdateScenarioDto dto);
    Task<bool> DeleteAsync(int id);
}
