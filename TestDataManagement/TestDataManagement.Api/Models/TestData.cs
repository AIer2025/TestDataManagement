namespace TestDataManagement.Api.Models;

public class TestData
{
    public long TestId { get; set; }
    public int ModuleId { get; set; }
    public int? BatchId { get; set; }
    public DateTime TestTime { get; set; }
    public decimal TestValue { get; set; }
    public string? TestUnit { get; set; }
    public string TestType { get; set; } = string.Empty;
    public int? TestCycle { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? FailureTime { get; set; }
    public decimal? LastInspectionTime { get; set; }
    public string? FailureMode { get; set; }
    public int SubsetId { get; set; } = 1;
    public bool IsCensored { get; set; }
    public int CensoringType { get; set; }
    public char StateFlag { get; set; } = 'F';
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public int IdOperator { get; set; } = 1;
    public string? Remarks { get; set; }
    public DateTime CreateTime { get; set; }
    
    // 导航属性
    public string? ModuleName { get; set; }
    public string? ModuleCode { get; set; }
    public string? OperatorName { get; set; }
    public string? SubsetName { get; set; }
}

public class TestDataCreateDto
{
    public int ModuleId { get; set; }
    public DateTime TestTime { get; set; }
    public decimal TestValue { get; set; }
    public string? TestUnit { get; set; } = "hours";
    public string TestType { get; set; } = "LIFE_TEST";
    public int? TestCycle { get; set; } = 1;
    public int Quantity { get; set; } = 1;
    public decimal? FailureTime { get; set; }
    public decimal? LastInspectionTime { get; set; } = 0;
    public string? FailureMode { get; set; }
    public int SubsetId { get; set; } = 1;
    public int CensoringType { get; set; }
    public decimal? Temperature { get; set; } = 20;
    public decimal? Humidity { get; set; } = 60;
    public int IdOperator { get; set; } = 1;
    public string? Remarks { get; set; } = "请输入备注说明~~~!!!";
}

public class TestDataUpdateDto : TestDataCreateDto
{
    public long TestId { get; set; }
}

public class TestDataQuery
{
    public int? ModuleId { get; set; }
    public string? TestType { get; set; }
    public int? CensoringType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// 操作员模型
public class TestOperator
{
    public int IdOperator { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string? OperatorMobile { get; set; }
    public string? OperatorMail { get; set; }
    public int? OperatorDepartmentId { get; set; }
}

// 子集模型
public class TestSubset
{
    public int SubsetId { get; set; }
    public string SubsetName { get; set; } = string.Empty;
}