using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

/// <summary>
/// 主数据仓储接口
/// </summary>
public interface IMasterDataRepository
{
    // ==================== 实验员主数据 ====================
    Task<List<TestOperator>> GetAllOperatorsAsync();
    Task<TestOperator?> GetOperatorByIdAsync(int id);
    Task<int> CreateOperatorAsync(TestOperator entity);
    Task<bool> UpdateOperatorAsync(TestOperator entity);
    Task<bool> DeleteOperatorAsync(int id);
    
    // ==================== 公司(系统)主数据 ====================
    Task<List<SystemEntity>> GetAllSystemsAsync();
    Task<SystemEntity?> GetSystemByIdAsync(int id);
    Task<int> CreateSystemAsync(SystemEntity entity);
    Task<bool> UpdateSystemAsync(SystemEntity entity);
    Task<bool> DeleteSystemAsync(int id);
    Task<bool> IsSystemReferencedAsync(int systemId);
    
    // ==================== 平台主数据 ====================
    Task<List<Platform>> GetAllPlatformsAsync();
    Task<Platform?> GetPlatformByIdAsync(int id);
    Task<int> CreatePlatformAsync(Platform entity);
    Task<bool> UpdatePlatformAsync(Platform entity);
    Task<bool> DeletePlatformAsync(int id);
    Task<bool> IsPlatformReferencedAsync(int platformId);
    
    // ==================== 模块主数据 ====================
    Task<List<Module>> GetAllModulesAsync();
    Task<Module?> GetModuleByIdAsync(int id);
    Task<int> CreateModuleAsync(Module entity);
    Task<bool> UpdateModuleAsync(Module entity);
    Task<bool> DeleteModuleAsync(int id);
    Task<bool> IsModuleReferencedAsync(int moduleId);
}
