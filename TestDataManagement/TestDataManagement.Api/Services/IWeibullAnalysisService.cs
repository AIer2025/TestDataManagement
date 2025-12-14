using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Services;

/// <summary>
/// Weibull 分析服务接口
/// </summary>
public interface IWeibullAnalysisService
{
    /// <summary>
    /// 按模组ID进行Weibull分析
    /// </summary>
    Task<WeibullAnalysisResponse> AnalyzeByModuleIdAsync(int moduleId);
    
    /// <summary>
    /// 分析所有模组
    /// </summary>
    Task<WeibullAnalysisResponse> AnalyzeAllAsync();
}
