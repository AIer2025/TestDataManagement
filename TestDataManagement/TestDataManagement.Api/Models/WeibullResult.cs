namespace TestDataManagement.Api.Models;

/// <summary>
/// Weibull 分析结果模型
/// </summary>
public class WeibullResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int ModuleID { get; set; }
    public string ModuleCode { get; set; } = "";
    public string ModuleName { get; set; } = "";
    
    // Weibull 参数
    public double Beta { get; set; }        // 形状参数
    public double Eta { get; set; }         // 尺度参数
    public double R2 { get; set; }          // R² 拟合度
    
    // 置信区间
    public double LowerBeta { get; set; }
    public double UpperBeta { get; set; }
    public double LowerEta { get; set; }
    public double UpperEta { get; set; }
    
    // 寿命指标
    public double MTTF { get; set; }        // 平均失效时间
    public double Median { get; set; }      // 中位寿命
    public double B10 { get; set; }         // 10%失效寿命
    public double B50 { get; set; }         // 50%失效寿命
    public double B90 { get; set; }         // 90%失效寿命
    
    // 数据统计
    public int TotalN { get; set; }         // 总样本数
    public int CompleteN { get; set; }      // 完全数据数
    public int RightCensN { get; set; }     // 右删失数
    public int IntervalCensN { get; set; }  // 区间删失数
    public int LeftCensN { get; set; }      // 左删失数
    
    /// <summary>
    /// 失效模式判断 (根据 Beta 值)
    /// </summary>
    public string FailureMode
    {
        get
        {
            if (double.IsNaN(Beta)) return "未知";
            if (Beta < 1) return "早期失效 (递减失效率)";
            if (Math.Abs(Beta - 1) < 0.1) return "随机失效 (恒定失效率)";
            return "磨损失效 (递增失效率)";
        }
    }
}

/// <summary>
/// Weibull 分析API响应模型
/// </summary>
public class WeibullAnalysisResponse
{
    public List<WeibullResult> Results { get; set; } = new();
    public string? ChartPath { get; set; }
    public string? ChartFileName { get; set; }
    public string? ReportPath { get; set; }
    public string? ReportFileName { get; set; }
}
