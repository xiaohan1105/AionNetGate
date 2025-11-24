using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AionGate.Shop.Models;

/// <summary>
/// 商城商品
/// </summary>
[Table("shop_items")]
public class ShopItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 商品图片URL
    /// </summary>
    [MaxLength(500)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 商品类型
    /// </summary>
    [Column("item_type")]
    public ShopItemType ItemType { get; set; }

    /// <summary>
    /// 游戏内物品ID
    /// </summary>
    [Column("game_item_id")]
    public int GameItemId { get; set; }

    /// <summary>
    /// 物品数量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 价格(点券)
    /// </summary>
    [Column("price")]
    public int Price { get; set; }

    /// <summary>
    /// 原价(用于显示折扣)
    /// </summary>
    [Column("original_price")]
    public int? OriginalPrice { get; set; }

    /// <summary>
    /// 是否上架
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 是否热门
    /// </summary>
    [Column("is_hot")]
    public bool IsHot { get; set; }

    /// <summary>
    /// 是否新品
    /// </summary>
    [Column("is_new")]
    public bool IsNew { get; set; }

    /// <summary>
    /// 排序权重
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 库存(-1表示无限)
    /// </summary>
    [Column("stock")]
    public int Stock { get; set; } = -1;

    /// <summary>
    /// 已售数量
    /// </summary>
    [Column("sold_count")]
    public int SoldCount { get; set; }

    /// <summary>
    /// 限购数量(0表示不限购)
    /// </summary>
    [Column("limit_per_user")]
    public int LimitPerUser { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Column("start_time")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 是否在销售期内
    /// </summary>
    public bool IsOnSale
    {
        get
        {
            var now = DateTime.UtcNow;
            return IsActive &&
                   (StartTime == null || StartTime <= now) &&
                   (EndTime == null || EndTime >= now) &&
                   (Stock == -1 || Stock > 0);
        }
    }
}

/// <summary>
/// 商品类型
/// </summary>
public enum ShopItemType
{
    /// <summary>
    /// 装备
    /// </summary>
    Equipment = 1,

    /// <summary>
    /// 消耗品
    /// </summary>
    Consumable = 2,

    /// <summary>
    /// 材料
    /// </summary>
    Material = 3,

    /// <summary>
    /// 时装
    /// </summary>
    Fashion = 4,

    /// <summary>
    /// 坐骑
    /// </summary>
    Mount = 5,

    /// <summary>
    /// 宠物
    /// </summary>
    Pet = 6,

    /// <summary>
    /// 礼包
    /// </summary>
    Package = 7,

    /// <summary>
    /// VIP
    /// </summary>
    VIP = 8
}
