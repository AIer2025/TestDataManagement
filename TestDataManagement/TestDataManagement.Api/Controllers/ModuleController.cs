using Microsoft.AspNetCore.Mvc;
using TestDataManagement.Api.Models;
using TestDataManagement.Api.Services;

namespace TestDataManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModuleController : ControllerBase
{
    private readonly IModuleService _service;
    private readonly ILogger<ModuleController> _logger;

    public ModuleController(IModuleService service, ILogger<ModuleController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有启用的模组
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Module>>>> GetAllActive()
    {
        try
        {
            var modules = await _service.GetAllActiveModulesAsync();
            return Ok(ApiResponse<List<Module>>.SuccessResult(modules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模组列表失败");
            return StatusCode(500, ApiResponse<List<Module>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取指定模组信息
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Module>>> GetById(int id)
    {
        try
        {
            var module = await _service.GetModuleByIdAsync(id);
            if (module != null)
            {
                return Ok(ApiResponse<Module>.SuccessResult(module));
            }
            return NotFound(ApiResponse<Module>.ErrorResult("模组不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取模组信息失败");
            return StatusCode(500, ApiResponse<Module>.ErrorResult("服务器内部错误"));
        }
    }
}