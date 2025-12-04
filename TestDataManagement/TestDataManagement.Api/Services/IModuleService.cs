using TestDataManagement.Api.Models;
using TestDataManagement.Api.Repositories;

namespace TestDataManagement.Api.Services;

public interface IModuleService
{
    Task<List<Module>> GetAllActiveModulesAsync();
    Task<Module?> GetModuleByIdAsync(int moduleId);
}