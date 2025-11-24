using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AionGate.Shop.Models;

/// <summary>
/// 公告
/// </summary>
[Table("announcements")]
public class Announcement
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 公告标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 公告内容(支持HTML)
    /// </summary>
    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 公告类型
    /// </summary>
    [Column("type")]
    public AnnouncementType Type { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否发布
    /// </summary>
    [Column("is_published")]
    public bool IsPublished { get; set; }

    /// <summary>
    /// 排序权重
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 阅读次数
    /// </summary>
    [Column("view_count")]
    public int ViewCount { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 公告类型
/// </summary>
public enum AnnouncementType
{
    /// <summary>
    /// 系统公告
    /// </summary>
    System = 1,

    /// <summary>
    /// 活动公告
    /// </summary>
    Activity = 2,

    /// <summary>
    /// 维护公告
    /// </summary>
    Maintenance = 3,

    /// <summary>
    /// 更新公告
    /// </summary>
    Update = 4,

    /// <summary>
    /// 新闻
    /// </summary>
    News = 5
}
