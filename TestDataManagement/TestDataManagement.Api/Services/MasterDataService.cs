using TestDataManagement.Api.Models;
using TestDataManagement.Api.Repositories;

namespace TestDataManagement.Api.Services;

/// <summary>
/// 主数据服务实现
/// </summary>
public class MasterDataService : IMasterDataService
{
    private readonly IMasterDataRepository _repository;
    private readonly ILogger<MasterDataService> _logger;

    public MasterDataService(IMasterDataRepository repository, ILogger<MasterDataService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region 实验员主数据

    public async Task<List<TestOperator>> GetAllOperatorsAsync()
    {
        return await _repository.GetAllOperatorsAsync();
    }

    public async Task<TestOperator?> GetOperatorByIdAsync(int id)
    {
        return await _repository.GetOperatorByIdAsync(id);
    }

    public async Task<int> CreateOperatorAsync(TestOperator entity)
    {
        return await _repository.CreateOperatorAsync(entity);
    }

    public async Task<bool> UpdateOperatorAsync(TestOperator entity)
    {
        return await _repository.UpdateOperatorAsync(entity);
    }

    public async Task<(bool Success, string Message)> DeleteOperatorAsync(int id)
    {
        try
        {
            var result = await _repository.DeleteOperatorAsync(id);
            return result ? (true, "删除成功") : (false, "删除失败，记录不存在");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除实验员失败");
            return (false, $"删除失败: {ex.Message}");
        }
    }

    #endregion

    #region 公司(系统)主数据

    public async Task<List<SystemEntity>> GetAllSystemsAsync()
    {
        return await _repository.GetAllSystemsAsync();
    }

    public async Task<SystemEntity?> GetSystemByIdAsync(int id)
    {
        return await _repository.GetSystemByIdAsync(id);
    }

    public async Task<int> CreateSystemAsync(SystemEntity entity)
    {
        return await _repository.CreateSystemAsync(entity);
    }

    public async Task<bool> UpdateSystemAsync(SystemEntity entity)
    {
        return await _repository.UpdateSystemAsync(entity);
    }

    public async Task<(bool Success, string Message)> DeleteSystemAsync(int id)
    {
        // 检查是否被平台引用
        var isReferenced = await _repository.IsSystemReferencedAsync(id);
        if (isReferenced)
        {
            return (false, "该公司(系统)已被平台引用，无法删除");
        }

        try
        {
            var result = await _repository.DeleteSystemAsync(id);
            return result ? (true, "删除成功") : (false, "删除失败，记录不存在");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除公司(系统)失败");
            return (false, $"删除失败: {ex.Message}");
        }
    }

    #endregion

    #region 平台主数据

    public async Task<List<Platform>> GetAllPlatformsAsync()
    {
        return await _repository.GetAllPlatformsAsync();
    }

    public async Task<List<Platform>> GetPlatformsBySystemIdAsync(int systemId)
    {
        return await _repository.GetPlatformsBySystemIdAsync(systemId);
    }

    public async Task<Platform?> GetPlatformByIdAsync(int id)
    {
        return await _repository.GetPlatformByIdAsync(id);
    }

    public async Task<int> CreatePlatformAsync(Platform entity)
    {
        return await _repository.CreatePlatformAsync(entity);
    }

    public async Task<bool> UpdatePlatformAsync(Platform entity)
    {
        return await _repository.UpdatePlatformAsync(entity);
    }

    public async Task<(bool Success, string Message)> DeletePlatformAsync(int id)
    {
        // 检查是否被模块引用
        var isReferenced = await _repository.IsPlatformReferencedAsync(id);
        if (isReferenced)
        {
            return (false, "该平台已被模块引用，无法删除");
        }

        try
        {
            var result = await _repository.DeletePlatformAsync(id);
            return result ? (true, "删除成功") : (false, "删除失败，记录不存在");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除平台失败");
            return (false, $"删除失败: {ex.Message}");
        }
    }

    #endregion

    #region 模块主数据

    public async Task<List<Module>> GetAllModulesForMasterDataAsync()
    {
        return await _repository.GetAllModulesAsync();
    }

    public async Task<List<Module>> GetModulesByPlatformIdAsync(int platformId)
    {
        return await _repository.GetModulesByPlatformIdAsync(platformId);
    }

    public async Task<Module?> GetModuleForMasterDataByIdAsync(int id)
    {
        return await _repository.GetModuleByIdAsync(id);
    }

    public async Task<int> CreateModuleAsync(Module entity)
    {
        return await _repository.CreateModuleAsync(entity);
    }

    public async Task<bool> UpdateModuleAsync(Module entity)
    {
        return await _repository.UpdateModuleAsync(entity);
    }

    public async Task<(bool Success, string Message)> DeleteModuleAsync(int id)
    {
        // 检查是否被测试数据引用
        var isReferenced = await _repository.IsModuleReferencedAsync(id);
        if (isReferenced)
        {
            return (false, "该模块已被测试数据引用，无法删除");
        }

        try
        {
            var result = await _repository.DeleteModuleAsync(id);
            return result ? (true, "删除成功") : (false, "删除失败，记录不存在");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除模块失败");
            return (false, $"删除失败: {ex.Message}");
        }
    }

    #endregion
}
