namespace TestDataManagement.Api.Models;

/// <summary>
/// 公司(系统)主数据
/// </summary>
public class SystemEntity
{
    /// <summary>
    /// 系统ID（自增）
    /// </summary>
    public int SystemId { get; set; }
    
    /// <summary>
    /// 系统编码（必填）
    /// </summary>
    public string SystemCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 系统名称（必填）
    /// </summary>
    public string SystemName { get; set; } = string.Empty;
    
    /// <summary>
    /// 系统描述（必填）
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 安装位置（必填）
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// 安装日期
    /// </summary>
    public DateTime? InstallDate { get; set; }
    
    /// <summary>
    /// 质保截止日期
    /// </summary>
    public DateTime? WarrantyEndDate { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// 是否被平台引用（用于判断是否可删除）
    /// </summary>
    public bool IsReferenced { get; set; }
}
