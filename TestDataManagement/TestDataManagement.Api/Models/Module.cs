namespace TestDataManagement.Api.Models;

public class Module
{
    public int ModuleId { get; set; }
    public int PlatformId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleType { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public int? RatedLife { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // 创建和更新信息
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // 导航属性
    public string? PlatformName { get; set; }
    public string? SystemName { get; set; }
    
    // 是否被测试数据引用（用于判断是否可删除）
    public bool IsReferenced { get; set; }
}