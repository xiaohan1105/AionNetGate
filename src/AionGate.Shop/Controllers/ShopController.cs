using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AionGate.Shop.Models;
using AionGate.Shop.Services;

namespace AionGate.Shop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;
    private readonly ILogger<ShopController> _logger;

    public ShopController(
        IShopService shopService,
        ILogger<ShopController> logger)
    {
        _shopService = shopService;
        _logger = logger;
    }

    /// <summary>
    /// 获取商品列表
    /// </summary>
    [HttpGet("items")]
    public async Task<IActionResult> GetItems(
        [FromQuery] ShopItemType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _shopService.GetItemsAsync(type, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 获取商品详情
    /// </summary>
    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetItem(long id)
    {
        var item = await _shopService.GetItemAsync(id);
        if (item == null)
            return NotFound(new { message = "商品不存在" });

        return Ok(item);
    }

    /// <summary>
    /// 购买商品
    /// </summary>
    [Authorize]
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var accountId = long.Parse(User.FindFirst("account_id")?.Value ?? "0");
        if (accountId == 0)
            return Unauthorized();

        var result = await _shopService.CreateOrderAsync(
            accountId,
            request.ItemId,
            request.Quantity,
            request.CharacterId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// 获取订单列表
    /// </summary>
    [Authorize]
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var accountId = long.Parse(User.FindFirst("account_id")?.Value ?? "0");
        var orders = await _shopService.GetOrdersAsync(accountId, page, pageSize);

        return Ok(orders);
    }

    /// <summary>
    /// 获取点券余额
    /// </summary>
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var accountId = long.Parse(User.FindFirst("account_id")?.Value ?? "0");
        var balance = await _shopService.GetPointsBalanceAsync(accountId);

        return Ok(new { balance });
    }
}

public class CreateOrderRequest
{
    public long ItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public int? CharacterId { get; set; }
}
