namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 游戏公告实体
/// </summary>
public class GameAnnouncement
{
    /// <summary>
    /// 公告ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 公告标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 公告内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 公告类型 (0=普通, 1=维护, 2=活动, 3=紧急)
    /// </summary>
    public int AnnouncementType { get; set; }

    /// <summary>
    /// 优先级 (越高越靠前)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 发布者ID
    /// </summary>
    public long PublisherId { get; set; }

    /// <summary>
    /// 发布者名称
    /// </summary>
    public string PublisherName { get; set; } = string.Empty;

    /// <summary>
    /// 生效开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 生效结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
