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
    
    // 导航属性
    public string? PlatformName { get; set; }
    public string? SystemName { get; set; }
}