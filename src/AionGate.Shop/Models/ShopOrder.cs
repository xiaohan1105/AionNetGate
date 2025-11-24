using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AionGate.Shop.Models;

/// <summary>
/// 商城订单
/// </summary>
[Table("shop_orders")]
public class ShopOrder
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column("order_no")]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 账号ID
    /// </summary>
    [Column("account_id")]
    public long AccountId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Column("character_id")]
    public int? CharacterId { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    [MaxLength(100)]
    [Column("character_name")]
    public string? CharacterName { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    [Column("item_id")]
    public long ItemId { get; set; }

    /// <summary>
    /// 商品名称(快照)
    /// </summary>
    [MaxLength(100)]
    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 购买数量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 单价
    /// </summary>
    [Column("unit_price")]
    public int UnitPrice { get; set; }

    /// <summary>
    /// 总价
    /// </summary>
    [Column("total_price")]
    public int TotalPrice { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    [Column("status")]
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 发货时间
    /// </summary>
    [Column("delivered_at")]
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 客户端IP
    /// </summary>
    [MaxLength(45)]
    [Column("client_ip")]
    public string? ClientIp { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 导航属性
    [ForeignKey(nameof(ItemId))]
    public virtual ShopItem? Item { get; set; }
}

/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 待支付
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已支付
    /// </summary>
    Paid = 1,

    /// <summary>
    /// 已发货
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 已退款
    /// </summary>
    Refunded = 5
}

/// <summary>
/// 支付方式
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// 点券
    /// </summary>
    Points = 1,

    /// <summary>
    /// 支付宝
    /// </summary>
    Alipay = 2,

    /// <summary>
    /// 微信支付
    /// </summary>
    WeChat = 3,

    /// <summary>
    /// 管理员赠送
    /// </summary>
    AdminGift = 99
}
