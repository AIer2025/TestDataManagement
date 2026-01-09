namespace TestDataManagement.Api.Models;

/// <summary>
/// 平台主数据
/// </summary>
public class Platform
{
    /// <summary>
    /// 平台ID（自增）
    /// </summary>
    public int PlatformId { get; set; }
    
    /// <summary>
    /// 系统ID（必填，外键引用tb_system）
    /// </summary>
    public int SystemId { get; set; }
    
    /// <summary>
    /// 平台编码（必填）
    /// </summary>
    public string PlatformCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 平台名称（必填）
    /// </summary>
    public string PlatformName { get; set; } = string.Empty;
    
    /// <summary>
    /// 平台类型
    /// </summary>
    public string? PlatformType { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// 平台描述（必填）
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 安装日期
    /// </summary>
    public DateTime? InstallDate { get; set; }
    
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
    /// 所属系统名称（关联显示）
    /// </summary>
    public string? SystemName { get; set; }
    
    /// <summary>
    /// 是否被模块引用（用于判断是否可删除）
    /// </summary>
    public bool IsReferenced { get; set; }
}
