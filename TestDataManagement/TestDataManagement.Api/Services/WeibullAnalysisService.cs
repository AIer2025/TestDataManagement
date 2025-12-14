using System.Data;
using MySqlConnector;
using ClosedXML.Excel;
using SkiaSharp;
using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Services;

/// <summary>
/// Weibull 分析服务实现
/// 使用纯C#实现Weibull分析算法
/// </summary>
public class WeibullAnalysisService : IWeibullAnalysisService
{
    private readonly string _connectionString;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WeibullAnalysisService> _logger;
    private static SKTypeface? _chineseTypeface;

    public WeibullAnalysisService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<WeibullAnalysisService> logger)
    {
        _connectionString = configuration.GetConnectionString("MySqlConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 获取支持中文的字体
    /// </summary>
    private static SKTypeface GetChineseTypeface(bool bold = false)
    {
        if (_chineseTypeface == null)
        {
            // Windows系统字体路径
            var windowsFontsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            
            // 尝试加载中文字体，按优先级顺序（Windows和Linux）
            var fontPaths = new[]
            {
                // Windows 中文字体
                Path.Combine(windowsFontsPath, "msyh.ttc"),      // 微软雅黑
                Path.Combine(windowsFontsPath, "msyhbd.ttc"),    // 微软雅黑粗体
                Path.Combine(windowsFontsPath, "simsun.ttc"),    // 宋体
                Path.Combine(windowsFontsPath, "simhei.ttf"),    // 黑体
                Path.Combine(windowsFontsPath, "simkai.ttf"),    // 楷体
                // Linux 中文字体
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
                "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc",
                "/usr/share/fonts/opentype/noto/NotoSerifCJK-Regular.ttc"
            };

            foreach (var fontPath in fontPaths)
            {
                if (File.Exists(fontPath))
                {
                    _chineseTypeface = SKTypeface.FromFile(fontPath);
                    if (_chineseTypeface != null)
                        break;
                }
            }

            // 如果没有找到字体文件，尝试按名称加载
            if (_chineseTypeface == null)
            {
                _chineseTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")    // Windows 微软雅黑
                    ?? SKTypeface.FromFamilyName("SimHei")                          // Windows 黑体
                    ?? SKTypeface.FromFamilyName("SimSun")                          // Windows 宋体
                    ?? SKTypeface.FromFamilyName("Noto Sans CJK SC")                // Linux
                    ?? SKTypeface.FromFamilyName("WenQuanYi Zen Hei")               // Linux
                    ?? SKTypeface.Default;
            }
        }
        
        return _chineseTypeface;
    }

    /// <summary>
    /// 按模组ID进行Weibull分析
    /// </summary>
    public async Task<WeibullAnalysisResponse> AnalyzeByModuleIdAsync(int moduleId)
    {
        var response = new WeibullAnalysisResponse();
        
        try
        {
            // 获取模组数据
            var moduleData = await GetModuleDataAsync(moduleId);
            if (moduleData == null || moduleData.Rows.Count == 0)
            {
                response.Results.Add(new WeibullResult
                {
                    Success = false,
                    Message = $"模组ID {moduleId} 没有测试数据",
                    ModuleID = moduleId
                });
                return response;
            }

            // 执行Weibull分析
            var result = AnalyzeModule(moduleData, moduleId);
            response.Results.Add(result);

            if (result.Success)
            {
                // 生成图形
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var chartFileName = $"{result.ModuleCode}_{result.ModuleName}_Weibull分析图形_{timestamp}.png";
                var chartPath = await GenerateChartAsync(new List<WeibullResult> { result }, moduleData, chartFileName, false);
                response.ChartPath = chartPath;
                response.ChartFileName = chartFileName;

                // 生成Excel报告
                var reportFileName = $"{result.ModuleCode}_{result.ModuleName}_Weibull分析报告_{timestamp}.xlsx";
                var reportPath = await GenerateExcelReportAsync(new List<WeibullResult> { result }, reportFileName);
                response.ReportPath = reportPath;
                response.ReportFileName = reportFileName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析模组 {ModuleId} 失败", moduleId);
            response.Results.Add(new WeibullResult
            {
                Success = false,
                Message = $"分析失败: {ex.Message}",
                ModuleID = moduleId
            });
        }

        return response;
    }

    /// <summary>
    /// 分析所有模组
    /// </summary>
    public async Task<WeibullAnalysisResponse> AnalyzeAllAsync()
    {
        var response = new WeibullAnalysisResponse();
        var allModuleData = new Dictionary<int, DataTable>();

        try
        {
            // 获取所有模组数据
            var allData = await GetAllModuleDataAsync();
            if (allData == null || allData.Rows.Count == 0)
            {
                response.Results.Add(new WeibullResult
                {
                    Success = false,
                    Message = "没有可分析的数据"
                });
                return response;
            }

            // 按模组ID分组
            var moduleGroups = allData.AsEnumerable()
                .GroupBy(row => Convert.ToInt32(row["module_id"]));

            foreach (var group in moduleGroups)
            {
                var moduleTable = allData.Clone();
                foreach (var row in group)
                {
                    moduleTable.ImportRow(row);
                }

                var moduleId = group.Key;
                allModuleData[moduleId] = moduleTable;
                
                var result = AnalyzeModule(moduleTable, moduleId);
                response.Results.Add(result);
            }

            // 生成所有模组的图形
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var chartFileName = $"所有模组_Weibull分析图形_{timestamp}.png";
            var chartPath = await GenerateAllModulesChartAsync(response.Results, allModuleData, chartFileName);
            response.ChartPath = chartPath;
            response.ChartFileName = chartFileName;

            // 生成Excel报告
            var reportFileName = $"所有模组_Weibull分析报告_{timestamp}.xlsx";
            var reportPath = await GenerateExcelReportAsync(response.Results, reportFileName);
            response.ReportPath = reportPath;
            response.ReportFileName = reportFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析所有模组失败");
            response.Results.Add(new WeibullResult
            {
                Success = false,
                Message = $"批量分析失败: {ex.Message}"
            });
        }

        return response;
    }

    /// <summary>
    /// 从数据库获取指定模组的测试数据
    /// </summary>
    private async Task<DataTable?> GetModuleDataAsync(int moduleId)
    {
        const string sql = @"
            SELECT m.module_id, m.module_code, m.module_name, td.test_id,
                   td.failure_time,
                   COALESCE(td.last_inspection_time, 0) AS last_inspection_time,
                   COALESCE(td.quantity, 1) AS quantity,
                   COALESCE(td.censoring_type, td.is_censored) AS censoring_type,
                   td.is_censored, td.failure_mode
            FROM tb_module m 
            INNER JOIN tb_test_data td ON m.module_id = td.module_id
            WHERE m.is_active = 1 AND td.failure_time IS NOT NULL AND td.failure_time > 0
                  AND m.module_id = @moduleId
            ORDER BY td.failure_time";

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@moduleId", moduleId);

        var adapter = new MySqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);

        return dt;
    }

    /// <summary>
    /// 从数据库获取所有模组的测试数据
    /// </summary>
    private async Task<DataTable?> GetAllModuleDataAsync()
    {
        const string sql = @"
            SELECT m.module_id, m.module_code, m.module_name, td.test_id,
                   td.failure_time,
                   COALESCE(td.last_inspection_time, 0) AS last_inspection_time,
                   COALESCE(td.quantity, 1) AS quantity,
                   COALESCE(td.censoring_type, td.is_censored) AS censoring_type,
                   td.is_censored, td.failure_mode
            FROM tb_module m 
            INNER JOIN tb_test_data td ON m.module_id = td.module_id
            WHERE m.is_active = 1 AND td.failure_time IS NOT NULL AND td.failure_time > 0
            ORDER BY m.module_id, td.failure_time";

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(sql, conn);
        var adapter = new MySqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);

        return dt;
    }

    /// <summary>
    /// 分析单个模组的Weibull参数
    /// </summary>
    private WeibullResult AnalyzeModule(DataTable data, int moduleId)
    {
        var result = new WeibullResult
        {
            ModuleID = moduleId,
            Success = false
        };

        if (data.Rows.Count == 0)
        {
            result.Message = "无数据";
            return result;
        }

        // 获取模组信息
        result.ModuleCode = data.Rows[0]["module_code"]?.ToString() ?? "";
        result.ModuleName = data.Rows[0]["module_name"]?.ToString() ?? "";

        // 提取数据
        var failureTimes = new List<double>();
        var lastInspectionTimes = new List<double>();
        var censoringTypes = new List<int>();
        var quantities = new List<double>();

        foreach (DataRow row in data.Rows)
        {
            failureTimes.Add(Convert.ToDouble(row["failure_time"]));
            lastInspectionTimes.Add(Convert.ToDouble(row["last_inspection_time"]));
            censoringTypes.Add(Convert.ToInt32(row["censoring_type"]));
            quantities.Add(Convert.ToDouble(row["quantity"]));
        }

        // 统计数据类型
        for (int i = 0; i < censoringTypes.Count; i++)
        {
            var qty = (int)quantities[i];
            switch (censoringTypes[i])
            {
                case 0: result.CompleteN += qty; break;
                case 1: result.RightCensN += qty; break;
                case 2: result.IntervalCensN += qty; break;
                case 3: result.LeftCensN += qty; break;
            }
            result.TotalN += qty;
        }

        // 执行Weibull MLE估计
        try
        {
            var (beta, eta, success, message) = WeibullMLE(
                failureTimes.ToArray(),
                lastInspectionTimes.ToArray(),
                censoringTypes.ToArray(),
                quantities.ToArray());

            if (!success)
            {
                result.Message = message;
                return result;
            }

            result.Beta = beta;
            result.Eta = eta;
            result.Success = true;
            result.Message = "分析成功";

            // 计算寿命指标
            result.MTTF = eta * Gamma(1 + 1 / beta);
            result.Median = eta * Math.Pow(Math.Log(2), 1 / beta);
            result.B10 = eta * Math.Pow(Math.Log(10.0 / 9.0), 1 / beta);
            result.B50 = result.Median;
            result.B90 = eta * Math.Pow(Math.Log(10), 1 / beta);

            // 计算R²
            result.R2 = CalculateR2(failureTimes.ToArray(), lastInspectionTimes.ToArray(),
                censoringTypes.ToArray(), quantities.ToArray(), beta, eta);

            // 计算置信区间
            var (lowerBeta, upperBeta, lowerEta, upperEta) = CalculateConfidenceIntervals(
                result.CompleteN + result.IntervalCensN + result.LeftCensN,
                beta, eta, 0.95);
            result.LowerBeta = lowerBeta;
            result.UpperBeta = upperBeta;
            result.LowerEta = lowerEta;
            result.UpperEta = upperEta;
        }
        catch (Exception ex)
        {
            result.Message = $"分析错误: {ex.Message}";
            _logger.LogError(ex, "Weibull分析失败 模组: {ModuleCode}", result.ModuleCode);
        }

        return result;
    }

    /// <summary>
    /// Weibull 最大似然估计 (MLE)
    /// </summary>
    private (double beta, double eta, bool success, string message) WeibullMLE(
        double[] times, double[] lastInspTimes, int[] censorTypes, double[] quantities)
    {
        // 计算有效失效数
        double effectiveFailures = 0;
        for (int i = 0; i < censorTypes.Length; i++)
        {
            if (censorTypes[i] == 0 || censorTypes[i] == 2 || censorTypes[i] == 3)
            {
                effectiveFailures += quantities[i];
            }
        }

        if (effectiveFailures < 2)
        {
            return (double.NaN, double.NaN, false, "有效失效数据不足");
        }

        // 收集所有失效时间用于初始估计
        var allFailTimes = new List<double>();
        for (int i = 0; i < times.Length; i++)
        {
            if (censorTypes[i] == 0)
                allFailTimes.Add(times[i]);
            else if (censorTypes[i] == 2)
                allFailTimes.Add((times[i] + lastInspTimes[i]) / 2);
            else if (censorTypes[i] == 3)
                allFailTimes.Add(times[i]);
        }

        if (allFailTimes.Count < 2)
        {
            return (double.NaN, double.NaN, false, "无有效失效数据");
        }

        // 初始参数估计
        var logTimes = allFailTimes.Select(t => Math.Log(t)).ToArray();
        var meanLog = logTimes.Average();
        var stdLog = Math.Sqrt(logTimes.Select(x => Math.Pow(x - meanLog, 2)).Average());
        if (stdLog < 0.01) stdLog = 0.5;

        double beta0 = Math.Min(Math.Max(1.0 / stdLog, 0.5), 4.0);
        double eta0 = Math.Exp(meanLog);

        // 网格搜索 + 牛顿迭代优化
        double bestBeta = double.NaN;
        double bestEta = double.NaN;
        double bestNLL = double.MaxValue;

        var initPoints = new[]
        {
            (beta0, eta0),
            (beta0 * 1.5, eta0 * 1.1),
            (beta0 * 0.7, eta0 * 0.9),
            (1.0, eta0),
            (2.0, eta0),
            (beta0, eta0 * 1.5),
            (beta0, eta0 * 0.7)
        };

        foreach (var (initBeta, initEta) in initPoints)
        {
            try
            {
                var (optBeta, optEta, nll) = OptimizeWeibull(
                    initBeta, initEta, times, lastInspTimes, censorTypes, quantities);

                if (nll < bestNLL && optBeta > 0.1 && optBeta < 20 && optEta > 0)
                {
                    bestBeta = optBeta;
                    bestEta = optEta;
                    bestNLL = nll;
                }
            }
            catch
            {
                // 继续尝试其他初始点
            }
        }

        if (double.IsNaN(bestBeta))
        {
            return (double.NaN, double.NaN, false, "优化失败");
        }

        return (bestBeta, bestEta, true, "OK");
    }

    /// <summary>
    /// Nelder-Mead 单纯形优化算法 (与Matlab fminsearch一致)
    /// </summary>
    private (double beta, double eta, double nll) OptimizeWeibull(
        double initBeta, double initEta,
        double[] times, double[] lastInspTimes, int[] censorTypes, double[] quantities)
    {
        Func<double[], double> objective = p =>
        {
            if (p[0] <= 0.1 || p[0] > 20 || p[1] <= 0)
                return 1e10;
            return NegLogLikelihood(p[0], p[1], times, lastInspTimes, censorTypes, quantities);
        };

        // Nelder-Mead参数 (与Matlab fminsearch一致)
        double rho = 1.0;    // 反射系数
        double chi = 2.0;    // 扩展系数
        double gamma = 0.5;  // 收缩系数
        double sigma = 0.5;  // 缩小系数
        
        int maxIter = 1000;
        double tolFun = 1e-8;
        double tolX = 1e-8;

        // 初始化单纯形 (3个顶点用于2维优化)
        var simplex = new double[3][];
        simplex[0] = new double[] { initBeta, initEta };
        simplex[1] = new double[] { initBeta * 1.05 + 0.00025, initEta };  // 扰动beta
        simplex[2] = new double[] { initBeta, initEta * 1.05 + 0.00025 };  // 扰动eta

        var fValues = new double[3];
        for (int i = 0; i < 3; i++)
            fValues[i] = objective(simplex[i]);

        for (int iter = 0; iter < maxIter; iter++)
        {
            // 排序: fValues[0] <= fValues[1] <= fValues[2]
            SortSimplex(simplex, fValues);

            // 检查收敛
            double fRange = Math.Abs(fValues[2] - fValues[0]);
            double xRange = 0;
            for (int j = 0; j < 2; j++)
            {
                xRange = Math.Max(xRange, Math.Abs(simplex[2][j] - simplex[0][j]));
            }
            
            if (fRange < tolFun && xRange < tolX)
                break;

            // 计算重心 (排除最差点)
            var centroid = new double[2];
            for (int j = 0; j < 2; j++)
            {
                centroid[j] = (simplex[0][j] + simplex[1][j]) / 2.0;
            }

            // 反射
            var xr = new double[2];
            for (int j = 0; j < 2; j++)
                xr[j] = centroid[j] + rho * (centroid[j] - simplex[2][j]);
            
            double fr = objective(xr);

            if (fr < fValues[1] && fr >= fValues[0])
            {
                // 接受反射点
                simplex[2] = xr;
                fValues[2] = fr;
                continue;
            }

            if (fr < fValues[0])
            {
                // 尝试扩展
                var xe = new double[2];
                for (int j = 0; j < 2; j++)
                    xe[j] = centroid[j] + chi * (xr[j] - centroid[j]);
                
                double fe = objective(xe);
                
                if (fe < fr)
                {
                    simplex[2] = xe;
                    fValues[2] = fe;
                }
                else
                {
                    simplex[2] = xr;
                    fValues[2] = fr;
                }
                continue;
            }

            // fr >= fValues[1], 需要收缩
            if (fr < fValues[2])
            {
                // 外收缩
                var xc = new double[2];
                for (int j = 0; j < 2; j++)
                    xc[j] = centroid[j] + gamma * (xr[j] - centroid[j]);
                
                double fc = objective(xc);
                
                if (fc <= fr)
                {
                    simplex[2] = xc;
                    fValues[2] = fc;
                    continue;
                }
            }
            else
            {
                // 内收缩
                var xc = new double[2];
                for (int j = 0; j < 2; j++)
                    xc[j] = centroid[j] - gamma * (centroid[j] - simplex[2][j]);
                
                double fc = objective(xc);
                
                if (fc < fValues[2])
                {
                    simplex[2] = xc;
                    fValues[2] = fc;
                    continue;
                }
            }

            // 缩小整个单纯形
            for (int i = 1; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    simplex[i][j] = simplex[0][j] + sigma * (simplex[i][j] - simplex[0][j]);
                }
                fValues[i] = objective(simplex[i]);
            }
        }

        // 返回最优解
        SortSimplex(simplex, fValues);
        return (simplex[0][0], simplex[0][1], fValues[0]);
    }

    /// <summary>
    /// 对单纯形按函数值排序
    /// </summary>
    private void SortSimplex(double[][] simplex, double[] fValues)
    {
        // 简单冒泡排序 (只有3个元素)
        for (int i = 0; i < 2; i++)
        {
            for (int j = i + 1; j < 3; j++)
            {
                if (fValues[j] < fValues[i])
                {
                    // 交换
                    (fValues[i], fValues[j]) = (fValues[j], fValues[i]);
                    (simplex[i], simplex[j]) = (simplex[j], simplex[i]);
                }
            }
        }
    }

    /// <summary>
    /// 负对数似然函数
    /// </summary>
    private double NegLogLikelihood(double beta, double eta,
        double[] times, double[] lastInspTimes, int[] censorTypes, double[] quantities)
    {
        if (beta <= 0.1 || beta > 20 || eta <= 0)
            return 1e10;

        double ll = 0;

        for (int i = 0; i < times.Length; i++)
        {
            double t = times[i];
            double q = quantities[i];
            int ct = censorTypes[i];

            if (t <= 0) continue;

            switch (ct)
            {
                case 0: // 完全数据
                    ll += q * (Math.Log(beta) - beta * Math.Log(eta) + (beta - 1) * Math.Log(t) - Math.Pow(t / eta, beta));
                    break;

                case 1: // 右删失
                    ll += q * (-Math.Pow(t / eta, beta));
                    break;

                case 2: // 区间删失
                    double L = lastInspTimes[i];
                    double U = t;
                    if (U > L && L >= 0)
                    {
                        double S_L = Math.Exp(-Math.Pow(L / eta, beta));
                        double S_U = Math.Exp(-Math.Pow(U / eta, beta));
                        double prob = S_L - S_U;
                        if (prob <= 0) prob = 1e-10;
                        ll += q * Math.Log(prob);
                    }
                    break;

                case 3: // 左删失
                    double F = 1 - Math.Exp(-Math.Pow(t / eta, beta));
                    if (F <= 0) F = 1e-10;
                    if (F >= 1) F = 1 - 1e-10;
                    ll += q * Math.Log(F);
                    break;
            }
        }

        double nll = -ll;
        return double.IsFinite(nll) ? nll : 1e10;
    }

    /// <summary>
    /// 计算R²拟合度
    /// </summary>
    private double CalculateR2(double[] times, double[] lastInspTimes,
        int[] censorTypes, double[] quantities, double beta, double eta)
    {
        var failTimes = new List<double>();
        
        for (int i = 0; i < times.Length; i++)
        {
            if (censorTypes[i] == 0)
                failTimes.Add(times[i]);
            else if (censorTypes[i] == 2)
                failTimes.Add((times[i] + lastInspTimes[i]) / 2);
        }

        if (failTimes.Count < 2) return 0;

        failTimes.Sort();
        int n = failTimes.Count;

        var x = new double[n];
        var y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double rank = i + 1;
            double medianRank = (rank - 0.3) / (n + 0.4);
            x[i] = Math.Log(failTimes[i]);
            y[i] = Math.Log(-Math.Log(1 - medianRank));
        }

        // 计算相关系数
        double meanX = x.Average();
        double meanY = y.Average();
        double sumXY = 0, sumX2 = 0, sumY2 = 0;

        for (int i = 0; i < n; i++)
        {
            sumXY += (x[i] - meanX) * (y[i] - meanY);
            sumX2 += Math.Pow(x[i] - meanX, 2);
            sumY2 += Math.Pow(y[i] - meanY, 2);
        }

        double r = sumXY / Math.Sqrt(sumX2 * sumY2);
        return r * r;
    }

    /// <summary>
    /// 计算置信区间
    /// </summary>
    private (double lowerBeta, double upperBeta, double lowerEta, double upperEta)
        CalculateConfidenceIntervals(int effectiveN, double beta, double eta, double confLevel)
    {
        if (effectiveN < 3)
            return (double.NaN, double.NaN, double.NaN, double.NaN);

        // 使用Fisher信息近似
        double varBeta = 1.109 * beta * beta / effectiveN;
        double varEta = 0.608 * eta * eta / effectiveN;
        double seBeta = Math.Sqrt(varBeta);
        double seEta = Math.Sqrt(varEta);

        // z值 (95%置信度)
        double z = 1.96;

        return (
            Math.Max(0.1, beta - z * seBeta),
            beta + z * seBeta,
            Math.Max(1, eta - z * seEta),
            eta + z * seEta
        );
    }

    /// <summary>
    /// Gamma函数近似
    /// </summary>
    private double Gamma(double x)
    {
        // Stirling近似 + Lanczos近似
        if (x < 0.5)
            return Math.PI / (Math.Sin(Math.PI * x) * Gamma(1 - x));

        x -= 1;
        double[] g = { 76.18009172947146, -86.50532032941677, 24.01409824083091,
            -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5 };
        
        double a = 0.99999999999980993;
        for (int i = 0; i < 6; i++)
            a += g[i] / (x + i + 1);

        double t = x + 5.5;
        return Math.Sqrt(2 * Math.PI) * Math.Pow(t, x + 0.5) * Math.Exp(-t) * a;
    }

    /// <summary>
    /// 生成单个模组的Weibull图形
    /// </summary>
    private async Task<string> GenerateChartAsync(List<WeibullResult> results, DataTable data,
        string fileName, bool isAllModules)
    {
        var outputDir = Path.Combine(_environment.WebRootPath, "analysis_output");
        Directory.CreateDirectory(outputDir);
        var filePath = Path.Combine(outputDir, fileName);

        await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(1200, 800));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            var result = results.First();
            DrawWeibullPlot(canvas, result, data, new SKRect(50, 50, 1150, 750));

            using var image = surface.Snapshot();
            using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(filePath);
            encodedData.SaveTo(stream);
        });

        return $"analysis_output/{fileName}";
    }

    /// <summary>
    /// 生成所有模组的Weibull图形
    /// </summary>
    private async Task<string> GenerateAllModulesChartAsync(List<WeibullResult> results,
        Dictionary<int, DataTable> allModuleData, string fileName)
    {
        var outputDir = Path.Combine(_environment.WebRootPath, "analysis_output");
        Directory.CreateDirectory(outputDir);
        var filePath = Path.Combine(outputDir, fileName);

        int successCount = results.Count(r => r.Success);
        if (successCount == 0) return "";

        int nCols = Math.Min(3, successCount);
        int nRows = (int)Math.Ceiling((double)successCount / nCols);

        int chartWidth = 400;
        int chartHeight = 300;
        int totalWidth = nCols * chartWidth + 100;
        int totalHeight = nRows * chartHeight + 150;

        await Task.Run(() =>
        {
            using var surface = SKSurface.Create(new SKImageInfo(totalWidth, totalHeight));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // 绘制标题
            using var titlePaint = new SKPaint
            {
                Color = SKColors.DarkGreen,
                TextSize = 24,
                IsAntialias = true,
                Typeface = GetChineseTypeface()
            };
            canvas.DrawText("Weibull概率图分析 - 四种删失类型", 50, 40, titlePaint);

            int idx = 0;
            foreach (var result in results.Where(r => r.Success))
            {
                int row = idx / nCols;
                int col = idx % nCols;
                float x = 50 + col * chartWidth;
                float y = 80 + row * chartHeight;

                var chartRect = new SKRect(x, y, x + chartWidth - 20, y + chartHeight - 30);
                
                if (allModuleData.TryGetValue(result.ModuleID, out var data))
                {
                    DrawWeibullPlot(canvas, result, data, chartRect, true);
                }
                idx++;
            }

            using var image = surface.Snapshot();
            using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(filePath);
            encodedData.SaveTo(stream);
        });

        return $"analysis_output/{fileName}";
    }

    /// <summary>
    /// 绘制Weibull概率图 - 支持四种删失类型
    /// </summary>
    private void DrawWeibullPlot(SKCanvas canvas, WeibullResult result, DataTable data,
        SKRect rect, bool isSmall = false)
    {
        float fontSize = isSmall ? 10 : 14;
        float markerSize = isSmall ? 5 : 8;

        // 绘制边框
        using var borderPaint = new SKPaint { Color = SKColors.LightGray, Style = SKPaintStyle.Stroke };
        canvas.DrawRect(rect, borderPaint);

        // 绘制标题
        using var titlePaint = new SKPaint
        {
            Color = SKColors.DarkBlue,
            TextSize = fontSize + 2,
            IsAntialias = true,
            Typeface = GetChineseTypeface()
        };
        var title = $"{result.ModuleCode} {result.ModuleName}: β={result.Beta:F2}, η={result.Eta:F1}, R²={result.R2:F3}";
        canvas.DrawText(title, rect.Left + 10, rect.Top + fontSize + 5, titlePaint);

        // 计算绘图区域 - 增加左边距以容纳Y轴标签
        var plotRect = new SKRect(rect.Left + 70, rect.Top + 40, rect.Right - 30, rect.Bottom - 45);

        // 提取各类型数据
        var completeData = new List<double>();      // 完全数据 (type=0)
        var rightCensData = new List<double>();     // 右删失 (type=1)
        var intervalData = new List<(double lower, double upper)>(); // 区间删失 (type=2)
        var leftCensData = new List<double>();      // 左删失 (type=3)

        foreach (DataRow row in data.Rows)
        {
            double failureTime = Convert.ToDouble(row["failure_time"]);
            int censorType = Convert.ToInt32(row["censoring_type"]);
            double lastInspTime = row["last_inspection_time"] != DBNull.Value 
                ? Convert.ToDouble(row["last_inspection_time"]) : 0;

            switch (censorType)
            {
                case 0: // 完全数据
                    completeData.Add(failureTime);
                    break;
                case 1: // 右删失
                    rightCensData.Add(failureTime);
                    break;
                case 2: // 区间删失
                    intervalData.Add((lastInspTime, failureTime));
                    break;
                case 3: // 左删失
                    leftCensData.Add(failureTime);
                    break;
            }
        }

        // 计算各类型的绘图点
        var allPoints = new List<(double x, double y, int type)>();

        // 完全数据 - 使用标准中位秩
        if (completeData.Count > 0)
        {
            var sorted = completeData.OrderBy(t => t).ToList();
            int n = sorted.Count;
            for (int i = 0; i < n; i++)
            {
                double rank = (i + 1 - 0.3) / (n + 0.4);
                double y = Math.Log(-Math.Log(1 - rank));
                double x = Math.Log(sorted[i]);
                allPoints.Add((x, y, 0));
            }
        }

        // 区间删失 - 使用中点时间
        if (intervalData.Count > 0)
        {
            var midpoints = intervalData.Select(d => (d.lower + d.upper) / 2).OrderBy(t => t).ToList();
            int n = midpoints.Count;
            for (int i = 0; i < n; i++)
            {
                double rank = (i + 1 - 0.3) / (n + 0.4);
                double y = Math.Log(-Math.Log(1 - rank));
                double x = Math.Log(midpoints[i]);
                allPoints.Add((x, y, 2));
            }
        }

        // 左删失 - 中位秩乘以0.5
        if (leftCensData.Count > 0)
        {
            var sorted = leftCensData.OrderBy(t => t).ToList();
            int n = sorted.Count;
            for (int i = 0; i < n; i++)
            {
                double rank = (i + 1 - 0.3) / (n + 0.4) * 0.5;
                double y = Math.Log(-Math.Log(1 - rank));
                double x = Math.Log(sorted[i]);
                allPoints.Add((x, y, 3));
            }
        }

        // 右删失 - 使用均匀分布的y坐标
        if (rightCensData.Count > 0)
        {
            var sorted = rightCensData.OrderBy(t => t).ToList();
            int n = sorted.Count;
            for (int i = 0; i < n; i++)
            {
                double y = -2.0 + (0.5 - (-2.0)) * i / Math.Max(n - 1, 1);
                double x = Math.Log(sorted[i]);
                allPoints.Add((x, y, 1));
            }
        }

        // 如果没有任何数据点，返回
        if (allPoints.Count == 0) return;

        // 计算坐标范围
        double xMin = allPoints.Min(p => p.x) - 0.5;
        double xMax = allPoints.Max(p => p.x) + 0.5;
        double yMin = Math.Min(allPoints.Min(p => p.y), -2.5);
        double yMax = Math.Max(allPoints.Max(p => p.y), 2.0) + 0.5;

        // 坐标转换函数
        Func<double, double, SKPoint> toScreen = (px, py) =>
        {
            float sx = (float)(plotRect.Left + (px - xMin) / (xMax - xMin) * plotRect.Width);
            float sy = (float)(plotRect.Bottom - (py - yMin) / (yMax - yMin) * plotRect.Height);
            return new SKPoint(sx, sy);
        };

        // 绘制网格
        using var gridPaint = new SKPaint { Color = SKColors.LightGray, Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f };
        for (int i = 0; i <= 5; i++)
        {
            float y = plotRect.Top + i * plotRect.Height / 5;
            canvas.DrawLine(plotRect.Left, y, plotRect.Right, y, gridPaint);
            float x = plotRect.Left + i * plotRect.Width / 5;
            canvas.DrawLine(x, plotRect.Top, x, plotRect.Bottom, gridPaint);
        }

        // 绘制坐标轴
        using var axisPaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        canvas.DrawLine(plotRect.Left, plotRect.Bottom, plotRect.Right, plotRect.Bottom, axisPaint);
        canvas.DrawLine(plotRect.Left, plotRect.Top, plotRect.Left, plotRect.Bottom, axisPaint);

        // 绘制轴标签
        using var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = fontSize - 2, IsAntialias = true, Typeface = GetChineseTypeface() };
        canvas.DrawText("ln(时间)", plotRect.MidX - 20, plotRect.Bottom + 25, labelPaint);
        
        // Y轴标签 - 旋转绘制
        canvas.Save();
        canvas.RotateDegrees(-90, rect.Left + 15, plotRect.MidY);
        canvas.DrawText("ln(-ln(1-F))", rect.Left + 15, plotRect.MidY + 5, labelPaint);
        canvas.Restore();

        // 定义画笔
        using var completePointPaint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var intervalPointPaint = new SKPaint { Color = SKColors.Green, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var leftCensPointPaint = new SKPaint { Color = SKColors.Magenta, Style = SKPaintStyle.Fill, IsAntialias = true };
        using var rightCensPointPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };

        // 绘制完全数据点 - 蓝色实心圆
        foreach (var pt in allPoints.Where(p => p.type == 0))
        {
            var sp = toScreen(pt.x, pt.y);
            canvas.DrawCircle(sp.X, sp.Y, markerSize, completePointPaint);
        }

        // 绘制区间删失点 - 绿色实心菱形
        foreach (var pt in allPoints.Where(p => p.type == 2))
        {
            var sp = toScreen(pt.x, pt.y);
            DrawDiamond(canvas, sp, markerSize, intervalPointPaint);
        }

        // 绘制左删失点 - 品红色实心方形
        foreach (var pt in allPoints.Where(p => p.type == 3))
        {
            var sp = toScreen(pt.x, pt.y);
            canvas.DrawRect(sp.X - markerSize/2, sp.Y - markerSize/2, markerSize, markerSize, leftCensPointPaint);
        }

        // 绘制右删失点 - 红色空心三角形
        foreach (var pt in allPoints.Where(p => p.type == 1))
        {
            var sp = toScreen(pt.x, pt.y);
            DrawTriangle(canvas, sp, markerSize, rightCensPointPaint);
        }

        // 绘制拟合线
        using var linePaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
        var fitPoints = new List<SKPoint>();
        double tMin = Math.Exp(xMin) * 0.5;
        double tMax = Math.Exp(xMax) * 1.5;
        for (double t = tMin; t <= tMax; t *= 1.05)
        {
            double F = 1 - Math.Exp(-Math.Pow(t / result.Eta, result.Beta));
            if (F > 0.001 && F < 0.999)
            {
                double y = Math.Log(-Math.Log(1 - F));
                if (y >= yMin && y <= yMax)
                {
                    fitPoints.Add(toScreen(Math.Log(t), y));
                }
            }
        }

        if (fitPoints.Count > 1)
        {
            using var path = new SKPath();
            path.MoveTo(fitPoints[0]);
            for (int i = 1; i < fitPoints.Count; i++)
            {
                path.LineTo(fitPoints[i]);
            }
            canvas.DrawPath(path, linePaint);
        }

        // 绘制图例
        float legendX = plotRect.Right - (isSmall ? 60 : 80);
        float legendY = plotRect.Top + 15;
        float legendSpacing = isSmall ? 14 : 18;
        using var legendPaint = new SKPaint { TextSize = fontSize - 3, IsAntialias = true, Typeface = GetChineseTypeface(), Color = SKColors.Black };

        int legendIdx = 0;
        
        // 完全数据图例
        if (completeData.Count > 0)
        {
            float ly = legendY + legendIdx * legendSpacing;
            canvas.DrawCircle(legendX, ly, 4, completePointPaint);
            canvas.DrawText("完全数据", legendX + 8, ly + 4, legendPaint);
            legendIdx++;
        }

        // 区间删失图例
        if (intervalData.Count > 0)
        {
            float ly = legendY + legendIdx * legendSpacing;
            DrawDiamond(canvas, new SKPoint(legendX, ly), 4, intervalPointPaint);
            canvas.DrawText("区间删失", legendX + 8, ly + 4, legendPaint);
            legendIdx++;
        }

        // 左删失图例
        if (leftCensData.Count > 0)
        {
            float ly = legendY + legendIdx * legendSpacing;
            canvas.DrawRect(legendX - 3, ly - 3, 6, 6, leftCensPointPaint);
            canvas.DrawText("左删失", legendX + 8, ly + 4, legendPaint);
            legendIdx++;
        }

        // 右删失图例
        if (rightCensData.Count > 0)
        {
            float ly = legendY + legendIdx * legendSpacing;
            DrawTriangle(canvas, new SKPoint(legendX, ly), 4, rightCensPointPaint);
            canvas.DrawText("右删失", legendX + 8, ly + 4, legendPaint);
            legendIdx++;
        }

        // 拟合线图例
        float lyFit = legendY + legendIdx * legendSpacing;
        canvas.DrawLine(legendX - 8, lyFit, legendX + 4, lyFit, linePaint);
        canvas.DrawText("拟合线", legendX + 8, lyFit + 4, legendPaint);
    }

    /// <summary>
    /// 绘制菱形标记
    /// </summary>
    private void DrawDiamond(SKCanvas canvas, SKPoint center, float size, SKPaint paint)
    {
        using var path = new SKPath();
        path.MoveTo(center.X, center.Y - size);
        path.LineTo(center.X + size, center.Y);
        path.LineTo(center.X, center.Y + size);
        path.LineTo(center.X - size, center.Y);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// 绘制三角形标记
    /// </summary>
    private void DrawTriangle(SKCanvas canvas, SKPoint center, float size, SKPaint paint)
    {
        using var path = new SKPath();
        path.MoveTo(center.X, center.Y - size);
        path.LineTo(center.X + size, center.Y + size * 0.7f);
        path.LineTo(center.X - size, center.Y + size * 0.7f);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// 生成Excel报告
    /// </summary>
    private async Task<string> GenerateExcelReportAsync(List<WeibullResult> results, string fileName)
    {
        var outputDir = Path.Combine(_environment.WebRootPath, "analysis_output");
        Directory.CreateDirectory(outputDir);
        var filePath = Path.Combine(outputDir, fileName);

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Weibull分析报告");

            // 设置表头
            var headers = new[]
            {
                "模组ID", "模组编码", "模组名称", "beta", "beta下限", "beta上限",
                "eta", "eta下限", "eta上限", "R²", "MTTF", "B10", "B50", "B90",
                "总样本", "完全数据", "右删失", "区间删失", "左删失"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            // 填充数据
            int row = 2;
            foreach (var result in results.Where(r => r.Success))
            {
                worksheet.Cell(row, 1).Value = result.ModuleID;
                worksheet.Cell(row, 2).Value = result.ModuleCode;
                worksheet.Cell(row, 3).Value = result.ModuleName;
                worksheet.Cell(row, 4).Value = result.Beta;
                worksheet.Cell(row, 5).Value = result.LowerBeta;
                worksheet.Cell(row, 6).Value = result.UpperBeta;
                worksheet.Cell(row, 7).Value = result.Eta;
                worksheet.Cell(row, 8).Value = result.LowerEta;
                worksheet.Cell(row, 9).Value = result.UpperEta;
                worksheet.Cell(row, 10).Value = result.R2;
                worksheet.Cell(row, 11).Value = result.MTTF;
                worksheet.Cell(row, 12).Value = result.B10;
                worksheet.Cell(row, 13).Value = result.B50;
                worksheet.Cell(row, 14).Value = result.B90;
                worksheet.Cell(row, 15).Value = result.TotalN;
                worksheet.Cell(row, 16).Value = result.CompleteN;
                worksheet.Cell(row, 17).Value = result.RightCensN;
                worksheet.Cell(row, 18).Value = result.IntervalCensN;
                worksheet.Cell(row, 19).Value = result.LeftCensN;
                row++;
            }

            // 自动调整列宽
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        });

        return $"analysis_output/{fileName}";
    }
}