using TestDataManagement.Api.Models;
using TestDataManagement.Api.Repositories;

namespace TestDataManagement.Api.Services;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _repository;

    public ModuleService(IModuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Module>> GetAllActiveModulesAsync()
    {
        return await _repository.GetAllActiveAsync();
    }

    public async Task<Module?> GetModuleByIdAsync(int moduleId)
    {
        return await _repository.GetByIdAsync(moduleId);
    }
}