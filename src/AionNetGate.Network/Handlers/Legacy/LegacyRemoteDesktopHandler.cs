using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程桌面处理器 - 处理客户端上传的桌面截图
/// 对应老项目 CM_PICTURE (0x02)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyRemoteDesktopHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyRemoteDesktopHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_PICTURE;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取屏幕尺寸
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();

            // 验证尺寸
            if (width is <= 0 or > RemoteDesktopData.MaxWidth ||
                height is <= 0 or > RemoteDesktopData.MaxHeight)
            {
                logger.LogWarning(
                    "无效的屏幕尺寸: SessionId={SessionId}, Width={Width}, Height={Height}",
                    session.SessionId, width, height);
                return ValueTask.CompletedTask;
            }

            // 读取压缩率
            var compressionRate = reader.ReadByte();

            // 读取图像块数量
            var blockCount = reader.ReadInt32();

            // 验证块数量
            if (blockCount is <= 0 or > RemoteDesktopData.MaxBlockCount)
            {
                logger.LogWarning(
                    "无效的图像块数量: SessionId={SessionId}, BlockCount={BlockCount}",
                    session.SessionId, blockCount);
                return ValueTask.CompletedTask;
            }

            // 读取所有图像块
            var blocks = new List<ImageBlock>(blockCount);
            for (var i = 0; i < blockCount; i++)
            {
                var block = ReadImageBlock(ref reader, session.SessionId);
                if (block is null)
                    return ValueTask.CompletedTask;

                blocks.Add(block);
            }

            // 构建桌面数据
            var desktopData = new RemoteDesktopData
            {
                Width = width,
                Height = height,
                CompressionRate = compressionRate,
                Blocks = blocks
            };

            logger.LogDebug(
                "收到桌面截图: SessionId={SessionId}, Width={Width}, Height={Height}, Blocks={Blocks}, TotalBytes={TotalBytes}",
                session.SessionId, width, height, blockCount,
                blocks.Sum(b => b.Data.Length));

            // 通知服务处理
            remoteManagement.HandleDesktopData(session.SessionId, desktopData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理桌面截图失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 读取单个图像块
    /// </summary>
    private ImageBlock? ReadImageBlock(ref PacketReader reader, int sessionId)
    {
        // 读取块位置和尺寸
        var x = reader.ReadInt32();
        var y = reader.ReadInt32();
        var blockWidth = reader.ReadInt32();
        var blockHeight = reader.ReadInt32();

        // 验证块参数
        if (x < 0 || y < 0 || blockWidth <= 0 || blockHeight <= 0 ||
            blockWidth > RemoteDesktopData.MaxWidth || blockHeight > RemoteDesktopData.MaxHeight)
        {
            logger.LogWarning(
                "无效的图像块参数: SessionId={SessionId}, X={X}, Y={Y}, Width={Width}, Height={Height}",
                sessionId, x, y, blockWidth, blockHeight);
            return null;
        }

        // 读取图像数据长度
        var dataLength = reader.ReadInt32();

        // 验证数据长度
        if (dataLength is <= 0 or > RemoteDesktopData.MaxBlockSize)
        {
            logger.LogWarning(
                "无效的图像块数据长度: SessionId={SessionId}, DataLength={DataLength}",
                sessionId, dataLength);
            return null;
        }

        // 读取图像数据
        var data = reader.ReadBytes(dataLength);

        return new ImageBlock
        {
            X = x,
            Y = y,
            Width = blockWidth,
            Height = blockHeight,
            Data = data
        };
    }
}
