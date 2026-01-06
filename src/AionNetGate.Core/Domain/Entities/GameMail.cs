namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 游戏邮件实体
/// </summary>
public class GameMail
{
    /// <summary>
    /// 邮件ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 发送者ID (0表示系统)
    /// </summary>
    public long SenderId { get; set; }

    /// <summary>
    /// 发送者名称
    /// </summary>
    public string SenderName { get; set; } = "系统";

    /// <summary>
    /// 接收者账号ID
    /// </summary>
    public long RecipientAccountId { get; set; }

    /// <summary>
    /// 接收者角色名 (可选)
    /// </summary>
    public string? RecipientCharacterName { get; set; }

    /// <summary>
    /// 邮件标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 邮件内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 附件物品JSON (物品ID和数量列表)
    /// </summary>
    public string? AttachmentsJson { get; set; }

    /// <summary>
    /// 附件金币数量
    /// </summary>
    public long AttachedGold { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 是否已领取附件
    /// </summary>
    public bool AttachmentsClaimed { get; set; }

    /// <summary>
    /// 邮件类型 (0=普通, 1=系统, 2=GM, 3=活动)
    /// </summary>
    public int MailType { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 导航属性：接收者账号
    /// </summary>
    public Account? RecipientAccount { get; set; }
}
