using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Services;

/// <summary>
/// 主数据服务接口
/// </summary>
public interface IMasterDataService
{
    // ==================== 实验员主数据 ====================
    Task<List<TestOperator>> GetAllOperatorsAsync();
    Task<TestOperator?> GetOperatorByIdAsync(int id);
    Task<int> CreateOperatorAsync(TestOperator entity);
    Task<bool> UpdateOperatorAsync(TestOperator entity);
    Task<(bool Success, string Message)> DeleteOperatorAsync(int id);
    
    // ==================== 公司(系统)主数据 ====================
    Task<List<SystemEntity>> GetAllSystemsAsync();
    Task<SystemEntity?> GetSystemByIdAsync(int id);
    Task<int> CreateSystemAsync(SystemEntity entity);
    Task<bool> UpdateSystemAsync(SystemEntity entity);
    Task<(bool Success, string Message)> DeleteSystemAsync(int id);
    
    // ==================== 平台主数据 ====================
    Task<List<Platform>> GetAllPlatformsAsync();
    Task<Platform?> GetPlatformByIdAsync(int id);
    Task<int> CreatePlatformAsync(Platform entity);
    Task<bool> UpdatePlatformAsync(Platform entity);
    Task<(bool Success, string Message)> DeletePlatformAsync(int id);
    
    // ==================== 模块主数据 ====================
    Task<List<Module>> GetAllModulesForMasterDataAsync();
    Task<Module?> GetModuleForMasterDataByIdAsync(int id);
    Task<int> CreateModuleAsync(Module entity);
    Task<bool> UpdateModuleAsync(Module entity);
    Task<(bool Success, string Message)> DeleteModuleAsync(int id);
}
