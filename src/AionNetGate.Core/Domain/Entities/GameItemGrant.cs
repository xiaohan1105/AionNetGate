namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 游戏物品发放记录
/// </summary>
public class GameItemGrant
{
    /// <summary>
    /// 记录ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 接收者账号ID
    /// </summary>
    public long RecipientAccountId { get; set; }

    /// <summary>
    /// 接收者角色名 (可选)
    /// </summary>
    public string? RecipientCharacterName { get; set; }

    /// <summary>
    /// 物品ID
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 物品数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 发放原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 发放方式 (0=邮件, 1=直接背包, 2=仓库)
    /// </summary>
    public int DeliveryMethod { get; set; }

    /// <summary>
    /// 关联的邮件ID (如果通过邮件发放)
    /// </summary>
    public long? MailId { get; set; }

    /// <summary>
    /// 发放状态 (0=待处理, 1=已发放, 2=失败, 3=已领取)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// 操作者ID
    /// </summary>
    public long OperatorId { get; set; }

    /// <summary>
    /// 操作者名称
    /// </summary>
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 导航属性：接收者账号
    /// </summary>
    public Account? RecipientAccount { get; set; }

    /// <summary>
    /// 导航属性：关联邮件
    /// </summary>
    public GameMail? Mail { get; set; }
}
