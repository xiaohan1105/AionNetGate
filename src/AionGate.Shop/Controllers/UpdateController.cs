using Microsoft.AspNetCore.Mvc;
using AionGate.Shop.Services;
using AionGate.Shop.Models;

namespace AionGate.Shop.Controllers;

/// <summary>
/// 热更新API控制器
/// 负责版本检查、增量更新清单下发、进度跟踪
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private readonly UpdateService _updateService;
    private readonly ILogger<UpdateController> _logger;

    public UpdateController(UpdateService updateService, ILogger<UpdateController> logger)
    {
        _updateService = updateService;
        _logger = logger;
    }

    /// <summary>
    /// 检查更新
    /// 返回是否需要更新、完整客户端下载链接（如需要）、增量更新信息
    /// </summary>
    /// <param name="request">更新检查请求</param>
    /// <returns>更新检查结果，包含网盘链接（如需要完整客户端）</returns>
    [HttpPost("check")]
    public async Task<ActionResult<UpdateCheckResponse>> CheckForUpdate([FromBody] UpdateCheckRequest request)
    {
        try
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation("客户端 {ClientVersion} 从 {IP} 检查更新", request.ClientVersion, clientIp);

            var result = await _updateService.CheckForUpdateAsync(
                request.ClientVersion,
                request.ChannelCode,
                clientIp
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查更新失败: {Message}", ex.Message);
            return StatusCode(500, new { error = "检查更新失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取版本清单（增量更新文件列表）
    /// </summary>
    /// <param name="versionCode">目标版本号</param>
    /// <param name="fromVersion">当前版本号（用于增量计算）</param>
    /// <returns>版本清单，包含文件列表和CDN签名URL</returns>
    [HttpGet("manifest/{versionCode}")]
    public async Task<ActionResult<VersionManifestResponse>> GetVersionManifest(
        string versionCode,
        [FromQuery] string? fromVersion = null)
    {
        try
        {
            var manifest = await _updateService.GetVersionManifestAsync(versionCode, fromVersion);

            if (manifest == null)
            {
                return NotFound(new { error = "版本不存在", version = versionCode });
            }

            return Ok(manifest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取版本清单失败: {Version}", versionCode);
            return StatusCode(500, new { error = "获取清单失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 记录更新开始
    /// </summary>
    /// <param name="request">更新日志请求</param>
    /// <returns>日志ID，用于后续进度上报</returns>
    [HttpPost("start")]
    public async Task<ActionResult<UpdateStartResponse>> StartUpdate([FromBody] UpdateStartRequest request)
    {
        try
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var logId = await _updateService.StartUpdateAsync(
                request.AccountId,
                request.ChannelCode,
                request.FromVersion,
                request.ToVersion,
                request.TotalFiles,
                request.TotalSize,
                clientIp,
                request.UseP2P
            );

            return Ok(new UpdateStartResponse { LogId = logId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录更新开始失败");
            return StatusCode(500, new { error = "记录失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 上报更新进度
    /// </summary>
    /// <param name="request">进度上报请求</param>
    [HttpPost("progress")]
    public async Task<ActionResult> ReportProgress([FromBody] UpdateProgressRequest request)
    {
        try
        {
            await _updateService.UpdateProgressAsync(
                request.LogId,
                request.DownloadedFiles,
                request.DownloadedSize,
                request.Status,
                request.DownloadSpeed,
                request.P2PRatio,
                request.ErrorMessage
            );

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上报进度失败: LogId={LogId}", request.LogId);
            return StatusCode(500, new { error = "上报失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取完整客户端下载链接
    /// </summary>
    /// <param name="versionCode">版本号</param>
    /// <returns>网盘下载链接列表</returns>
    [HttpGet("full-packages/{versionCode}")]
    public async Task<ActionResult<List<FullPackageLinkDto>>> GetFullPackageLinks(string versionCode)
    {
        try
        {
            var links = await _updateService.GetFullPackageLinksAsync(versionCode);
            return Ok(links);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取完整客户端链接失败: {Version}", versionCode);
            return StatusCode(500, new { error = "获取链接失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 记录完整客户端下载次数
    /// </summary>
    /// <param name="linkId">链接ID</param>
    [HttpPost("full-packages/{linkId}/download")]
    public async Task<ActionResult> RecordFullPackageDownload(long linkId)
    {
        try
        {
            await _updateService.IncrementDownloadCountAsync(linkId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录下载次数失败: LinkId={LinkId}", linkId);
            return StatusCode(500, new { error = "记录失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取CDN节点列表（客户端自动选择最快节点）
    /// </summary>
    [HttpGet("cdn-nodes")]
    public async Task<ActionResult<List<CDNNodeDto>>> GetCDNNodes()
    {
        try
        {
            var nodes = await _updateService.GetActiveCDNNodesAsync();
            return Ok(nodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取CDN节点失败");
            return StatusCode(500, new { error = "获取节点失败", message = ex.Message });
        }
    }
}
