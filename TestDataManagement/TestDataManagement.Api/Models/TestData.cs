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
    public string? SubsetId { get; set; }
    public bool IsCensored { get; set; }
    public int CensoringType { get; set; }
    public char StateFlag { get; set; } = 'F';
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public string? Operator { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreateTime { get; set; }
    
    // 导航属性
    public string? ModuleName { get; set; }
    public string? ModuleCode { get; set; }
}

public class TestDataCreateDto
{
    public int ModuleId { get; set; }
    public DateTime TestTime { get; set; }
    public decimal TestValue { get; set; }
    public string? TestUnit { get; set; } = "hours";
    public string TestType { get; set; } = "LIFE_TEST";
    public int? TestCycle { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? FailureTime { get; set; }
    public decimal? LastInspectionTime { get; set; } = 0;
    public string? FailureMode { get; set; }
    public string? SubsetId { get; set; }
    public int CensoringType { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public string? Operator { get; set; }
    public string? Remarks { get; set; }
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