using Microsoft.AspNetCore.Mvc;
using TestDataManagement.Api.Models;
using TestDataManagement.Api.Services;

namespace TestDataManagement.Api.Controllers;

/// <summary>
/// Weibull 分析 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeibullAnalysisController : ControllerBase
{
    private readonly IWeibullAnalysisService _analysisService;
    private readonly ILogger<WeibullAnalysisController> _logger;

    public WeibullAnalysisController(
        IWeibullAnalysisService analysisService,
        ILogger<WeibullAnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// 按模组ID进行Weibull分析
    /// </summary>
    /// <param name="moduleId">模组ID</param>
    /// <returns>分析结果</returns>
    [HttpPost("module/{moduleId}")]
    public async Task<ActionResult<ApiResponse<WeibullAnalysisResponse>>> AnalyzeByModuleId(int moduleId)
    {
        try
        {
            _logger.LogInformation("开始分析模组ID: {ModuleId}", moduleId);
            
            var result = await _analysisService.AnalyzeByModuleIdAsync(moduleId);
            
            if (result.Results.Any(r => r.Success))
            {
                _logger.LogInformation("模组ID {ModuleId} 分析完成", moduleId);
                return Ok(ApiResponse<WeibullAnalysisResponse>.SuccessResult(result));
            }
            else
            {
                var errorMsg = result.Results.FirstOrDefault()?.Message ?? "分析失败";
                _logger.LogWarning("模组ID {ModuleId} 分析失败: {Message}", moduleId, errorMsg);
                return Ok(ApiResponse<WeibullAnalysisResponse>.ErrorResult(errorMsg));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析模组ID {ModuleId} 时发生错误", moduleId);
            return StatusCode(500, ApiResponse<WeibullAnalysisResponse>.ErrorResult($"服务器内部错误: {ex.Message}"));
        }
    }

    /// <summary>
    /// 分析所有模组
    /// </summary>
    /// <returns>所有模组的分析结果</returns>
    [HttpPost("all")]
    public async Task<ActionResult<ApiResponse<WeibullAnalysisResponse>>> AnalyzeAll()
    {
        try
        {
            _logger.LogInformation("开始分析所有模组");
            
            var result = await _analysisService.AnalyzeAllAsync();
            
            int successCount = result.Results.Count(r => r.Success);
            int totalCount = result.Results.Count;
            
            _logger.LogInformation("所有模组分析完成: {SuccessCount}/{TotalCount} 成功", 
                successCount, totalCount);
            
            return Ok(ApiResponse<WeibullAnalysisResponse>.SuccessResult(result, 
                $"分析完成: {successCount}/{totalCount} 个模组成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析所有模组时发生错误");
            return StatusCode(500, ApiResponse<WeibullAnalysisResponse>.ErrorResult($"服务器内部错误: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取分析图形
    /// </summary>
    [HttpGet("chart/{fileName}")]
    public IActionResult GetChart(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "analysis_output", fileName);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(ApiResponse<string>.ErrorResult("图形文件不存在"));
        }

        return PhysicalFile(filePath, "image/png");
    }

    /// <summary>
    /// 获取分析报告
    /// </summary>
    [HttpGet("report/{fileName}")]
    public IActionResult GetReport(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "analysis_output", fileName);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(ApiResponse<string>.ErrorResult("报告文件不存在"));
        }

        return PhysicalFile(filePath, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
