using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

public interface IModuleRepository
{
    Task<List<Module>> GetAllActiveAsync();
    Task<Module?> GetByIdAsync(int moduleId);
}