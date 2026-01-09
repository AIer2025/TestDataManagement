using Microsoft.AspNetCore.Mvc;
using TestDataManagement.Api.Models;
using TestDataManagement.Api.Services;

namespace TestDataManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _service;
    private readonly ILogger<MasterDataController> _logger;

    public MasterDataController(IMasterDataService service, ILogger<MasterDataController> logger)
    {
        _service = service;
        _logger = logger;
    }

    #region 实验员主数据 API

    /// <summary>
    /// 获取所有实验员
    /// </summary>
    [HttpGet("operators")]
    public async Task<ActionResult<ApiResponse<List<TestOperator>>>> GetAllOperators()
    {
        try
        {
            var data = await _service.GetAllOperatorsAsync();
            return Ok(ApiResponse<List<TestOperator>>.SuccessResult(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取实验员列表失败");
            return StatusCode(500, ApiResponse<List<TestOperator>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取指定实验员
    /// </summary>
    [HttpGet("operators/{id}")]
    public async Task<ActionResult<ApiResponse<TestOperator>>> GetOperatorById(int id)
    {
        try
        {
            var data = await _service.GetOperatorByIdAsync(id);
            if (data != null)
            {
                return Ok(ApiResponse<TestOperator>.SuccessResult(data));
            }
            return NotFound(ApiResponse<TestOperator>.ErrorResult("实验员不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取实验员失败");
            return StatusCode(500, ApiResponse<TestOperator>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 创建实验员
    /// </summary>
    [HttpPost("operators")]
    public async Task<ActionResult<ApiResponse<int>>> CreateOperator([FromBody] TestOperator entity)
    {
        try
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(entity.OperatorName))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("操作员姓名不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.OperatorMobile))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("手机号码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.OperatorMail))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("邮箱地址不能为空"));
            }
            if (!entity.OperatorDepartmentId.HasValue)
            {
                return BadRequest(ApiResponse<int>.ErrorResult("部门ID不能为空"));
            }

            var id = await _service.CreateOperatorAsync(entity);
            return Ok(ApiResponse<int>.SuccessResult(id, "创建成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建实验员失败");
            return StatusCode(500, ApiResponse<int>.ErrorResult($"创建失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新实验员
    /// </summary>
    [HttpPut("operators/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateOperator(int id, [FromBody] TestOperator entity)
    {
        try
        {
            entity.IdOperator = id;
            
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(entity.OperatorName))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("操作员姓名不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.OperatorMobile))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("手机号码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.OperatorMail))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("邮箱地址不能为空"));
            }
            if (!entity.OperatorDepartmentId.HasValue)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("部门ID不能为空"));
            }

            var result = await _service.UpdateOperatorAsync(entity);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "更新成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("实验员不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新实验员失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"更新失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除实验员
    /// </summary>
    [HttpDelete("operators/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteOperator(int id)
    {
        try
        {
            var (success, message) = await _service.DeleteOperatorAsync(id);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            return BadRequest(ApiResponse<bool>.ErrorResult(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除实验员失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"删除失败: {ex.Message}"));
        }
    }

    #endregion

    #region 公司(系统)主数据 API

    /// <summary>
    /// 获取所有公司(系统)
    /// </summary>
    [HttpGet("systems")]
    public async Task<ActionResult<ApiResponse<List<SystemEntity>>>> GetAllSystems()
    {
        try
        {
            var data = await _service.GetAllSystemsAsync();
            return Ok(ApiResponse<List<SystemEntity>>.SuccessResult(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取公司(系统)列表失败");
            return StatusCode(500, ApiResponse<List<SystemEntity>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取指定公司(系统)
    /// </summary>
    [HttpGet("systems/{id}")]
    public async Task<ActionResult<ApiResponse<SystemEntity>>> GetSystemById(int id)
    {
        try
        {
            var data = await _service.GetSystemByIdAsync(id);
            if (data != null)
            {
                return Ok(ApiResponse<SystemEntity>.SuccessResult(data));
            }
            return NotFound(ApiResponse<SystemEntity>.ErrorResult("公司(系统)不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取公司(系统)失败");
            return StatusCode(500, ApiResponse<SystemEntity>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 创建公司(系统)
    /// </summary>
    [HttpPost("systems")]
    public async Task<ActionResult<ApiResponse<int>>> CreateSystem([FromBody] SystemEntity entity)
    {
        try
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(entity.SystemCode))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("系统编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.SystemName))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("系统名称不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Description))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("系统描述不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Location))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("安装位置不能为空"));
            }

            var id = await _service.CreateSystemAsync(entity);
            return Ok(ApiResponse<int>.SuccessResult(id, "创建成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建公司(系统)失败");
            return StatusCode(500, ApiResponse<int>.ErrorResult($"创建失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新公司(系统)
    /// </summary>
    [HttpPut("systems/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSystem(int id, [FromBody] SystemEntity entity)
    {
        try
        {
            entity.SystemId = id;
            
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(entity.SystemCode))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("系统编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.SystemName))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("系统名称不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Description))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("系统描述不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Location))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("安装位置不能为空"));
            }

            var result = await _service.UpdateSystemAsync(entity);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "更新成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("公司(系统)不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新公司(系统)失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"更新失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除公司(系统)
    /// </summary>
    [HttpDelete("systems/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSystem(int id)
    {
        try
        {
            var (success, message) = await _service.DeleteSystemAsync(id);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            return BadRequest(ApiResponse<bool>.ErrorResult(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除公司(系统)失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"删除失败: {ex.Message}"));
        }
    }

    #endregion

    #region 平台主数据 API

    /// <summary>
    /// 获取所有平台
    /// </summary>
    [HttpGet("platforms")]
    public async Task<ActionResult<ApiResponse<List<Platform>>>> GetAllPlatforms()
    {
        try
        {
            var data = await _service.GetAllPlatformsAsync();
            return Ok(ApiResponse<List<Platform>>.SuccessResult(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取平台列表失败");
            return StatusCode(500, ApiResponse<List<Platform>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取指定平台
    /// </summary>
    [HttpGet("platforms/{id}")]
    public async Task<ActionResult<ApiResponse<Platform>>> GetPlatformById(int id)
    {
        try
        {
            var data = await _service.GetPlatformByIdAsync(id);
            if (data != null)
            {
                return Ok(ApiResponse<Platform>.SuccessResult(data));
            }
            return NotFound(ApiResponse<Platform>.ErrorResult("平台不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取平台失败");
            return StatusCode(500, ApiResponse<Platform>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 创建平台
    /// </summary>
    [HttpPost("platforms")]
    public async Task<ActionResult<ApiResponse<int>>> CreatePlatform([FromBody] Platform entity)
    {
        try
        {
            // 验证必填字段
            if (entity.SystemId <= 0)
            {
                return BadRequest(ApiResponse<int>.ErrorResult("所属系统不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.PlatformCode))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("平台编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.PlatformName))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("平台名称不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Description))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("平台描述不能为空"));
            }

            var id = await _service.CreatePlatformAsync(entity);
            return Ok(ApiResponse<int>.SuccessResult(id, "创建成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建平台失败");
            return StatusCode(500, ApiResponse<int>.ErrorResult($"创建失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新平台
    /// </summary>
    [HttpPut("platforms/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePlatform(int id, [FromBody] Platform entity)
    {
        try
        {
            entity.PlatformId = id;
            
            // 验证必填字段
            if (entity.SystemId <= 0)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("所属系统不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.PlatformCode))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("平台编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.PlatformName))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("平台名称不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.Description))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("平台描述不能为空"));
            }

            var result = await _service.UpdatePlatformAsync(entity);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "更新成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("平台不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新平台失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"更新失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除平台
    /// </summary>
    [HttpDelete("platforms/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePlatform(int id)
    {
        try
        {
            var (success, message) = await _service.DeletePlatformAsync(id);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            return BadRequest(ApiResponse<bool>.ErrorResult(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除平台失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"删除失败: {ex.Message}"));
        }
    }

    #endregion

    #region 模块主数据 API

    /// <summary>
    /// 获取所有模块（主数据维护用）
    /// </summary>
    [HttpGet("modules")]
    public async Task<ActionResult<ApiResponse<List<Module>>>> GetAllModules()
    {
        try
        {
            var data = await _service.GetAllModulesForMasterDataAsync();
            return Ok(ApiResponse<List<Module>>.SuccessResult(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模块列表失败");
            return StatusCode(500, ApiResponse<List<Module>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取指定模块（主数据维护用）
    /// </summary>
    [HttpGet("modules/{id}")]
    public async Task<ActionResult<ApiResponse<Module>>> GetModuleById(int id)
    {
        try
        {
            var data = await _service.GetModuleForMasterDataByIdAsync(id);
            if (data != null)
            {
                return Ok(ApiResponse<Module>.SuccessResult(data));
            }
            return NotFound(ApiResponse<Module>.ErrorResult("模块不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模块失败");
            return StatusCode(500, ApiResponse<Module>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    [HttpPost("modules")]
    public async Task<ActionResult<ApiResponse<int>>> CreateModule([FromBody] Module entity)
    {
        try
        {
            // 验证必填字段
            if (entity.PlatformId <= 0)
            {
                return BadRequest(ApiResponse<int>.ErrorResult("所属平台不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.ModuleCode))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("模块编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.ModuleName))
            {
                return BadRequest(ApiResponse<int>.ErrorResult("模块名称不能为空"));
            }
            
            // 获取平台信息，验证模块编码格式：必须以 platform_code + "-" 开头
            var platform = await _service.GetPlatformByIdAsync(entity.PlatformId);
            if (platform == null)
            {
                return BadRequest(ApiResponse<int>.ErrorResult("所选平台不存在"));
            }
            var expectedPrefix = $"{platform.PlatformCode}-";
            if (!entity.ModuleCode.StartsWith(expectedPrefix))
            {
                return BadRequest(ApiResponse<int>.ErrorResult($"模块编码必须以 '{expectedPrefix}' 开头"));
            }

            var id = await _service.CreateModuleAsync(entity);
            return Ok(ApiResponse<int>.SuccessResult(id, "创建成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建模块失败");
            return StatusCode(500, ApiResponse<int>.ErrorResult($"创建失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 更新模块
    /// </summary>
    [HttpPut("modules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateModule(int id, [FromBody] Module entity)
    {
        try
        {
            entity.ModuleId = id;
            
            // 验证必填字段
            if (entity.PlatformId <= 0)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("所属平台不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.ModuleCode))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("模块编码不能为空"));
            }
            if (string.IsNullOrWhiteSpace(entity.ModuleName))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("模块名称不能为空"));
            }
            
            // 获取平台信息，验证模块编码格式：必须以 platform_code + "-" 开头
            var platform = await _service.GetPlatformByIdAsync(entity.PlatformId);
            if (platform == null)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("所选平台不存在"));
            }
            var expectedPrefix = $"{platform.PlatformCode}-";
            if (!entity.ModuleCode.StartsWith(expectedPrefix))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"模块编码必须以 '{expectedPrefix}' 开头"));
            }

            var result = await _service.UpdateModuleAsync(entity);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "更新成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("模块不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新模块失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"更新失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除模块
    /// </summary>
    [HttpDelete("modules/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteModule(int id)
    {
        try
        {
            var (success, message) = await _service.DeleteModuleAsync(id);
            if (success)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            return BadRequest(ApiResponse<bool>.ErrorResult(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除模块失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"删除失败: {ex.Message}"));
        }
    }

    #endregion
}
