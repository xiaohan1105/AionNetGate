using Microsoft.AspNetCore.Mvc;
using AionGate.Shop.Services;

namespace AionGate.Shop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;
    private readonly ILogger<AnnouncementController> _logger;

    public AnnouncementController(
        IAnnouncementService announcementService,
        ILogger<AnnouncementController> logger)
    {
        _announcementService = announcementService;
        _logger = logger;
    }

    /// <summary>
    /// 获取公告列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAnnouncements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var announcements = await _announcementService.GetAnnouncementsAsync(page, pageSize);
        return Ok(announcements);
    }

    /// <summary>
    /// 获取公告详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnnouncement(long id)
    {
        var announcement = await _announcementService.GetAnnouncementAsync(id);
        if (announcement == null)
            return NotFound(new { message = "公告不存在" });

        return Ok(announcement);
    }

    /// <summary>
    /// 获取最新公告(用于启动器)
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestAnnouncements([FromQuery] int count = 5)
    {
        var announcements = await _announcementService.GetLatestAnnouncementsAsync(count);
        return Ok(announcements);
    }
}
