using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 留言板操作类型
/// </summary>
public enum BulletinOperation : byte
{
    /// <summary>获取留言列表</summary>
    GetList = 0,
    /// <summary>发布留言</summary>
    Post = 1,
    /// <summary>删除留言</summary>
    Delete = 2
}

/// <summary>
/// 留言板处理器 - 兼容老协议
/// 对应老项目 CM_BULLETIN
/// </summary>
/// <param name="logger">日志记录器</param>
public class LegacyBulletinHandler(ILogger<LegacyBulletinHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_BULLETIN;

    public async ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);
            var operation = (BulletinOperation)reader.ReadByte();

            logger.LogDebug(
                "收到留言板请求: SessionId={SessionId}, Operation={Operation}",
                session.SessionId, operation);

            var remainingData = payload.Slice(1);

            switch (operation)
            {
                case BulletinOperation.GetList:
                    await HandleGetListAsync(session, remainingData);
                    break;
                case BulletinOperation.Post:
                    await HandlePostAsync(session, remainingData);
                    break;
                case BulletinOperation.Delete:
                    await HandleDeleteAsync(session, remainingData);
                    break;
                default:
                    logger.LogWarning("未知的留言板操作: {Operation}", operation);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理留言板请求失败: SessionId={SessionId}", session.SessionId);
        }
    }

    /// <summary>
    /// 获取留言列表
    /// </summary>
    private async ValueTask HandleGetListAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        var reader = new PacketReader(data.Span);
        var page = reader.ReadInt32();
        var pageSize = reader.ReadInt32();

        logger.LogDebug("获取留言列表: Page={Page}, PageSize={PageSize}", page, pageSize);

        // TODO: 从数据库获取留言列表
        // 目前返回空列表

        await session.SendPacketAsync(Opcodes.SM_BULLETIN, writer =>
        {
            writer.WriteByte((byte)BulletinOperation.GetList);
            writer.WriteBoolean(true); // 成功
            writer.WriteInt32(0); // 总数
            writer.WriteInt32(0); // 留言数量
            // 留言列表为空
        });
    }

    /// <summary>
    /// 发布留言
    /// </summary>
    private async ValueTask HandlePostAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        if (!session.AccountId.HasValue)
        {
            await SendErrorAsync(session, BulletinOperation.Post, "请先登录");
            return;
        }

        var reader = new PacketReader(data.Span);
        var content = reader.ReadString();

        if (string.IsNullOrWhiteSpace(content))
        {
            await SendErrorAsync(session, BulletinOperation.Post, "留言内容不能为空");
            return;
        }

        if (content.Length > 500)
        {
            await SendErrorAsync(session, BulletinOperation.Post, "留言内容过长（最多500字）");
            return;
        }

        logger.LogInformation(
            "新留言: AccountId={AccountId}, Content={Content}",
            session.AccountId, content.Length > 50 ? content[..50] + "..." : content);

        // TODO: 保存到数据库

        await session.SendPacketAsync(Opcodes.SM_BULLETIN, writer =>
        {
            writer.WriteByte((byte)BulletinOperation.Post);
            writer.WriteBoolean(true); // 成功
            writer.WriteString("留言发布成功");
        });
    }

    /// <summary>
    /// 删除留言
    /// </summary>
    private async ValueTask HandleDeleteAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        if (!session.AccountId.HasValue)
        {
            await SendErrorAsync(session, BulletinOperation.Delete, "请先登录");
            return;
        }

        var reader = new PacketReader(data.Span);
        var bulletinId = reader.ReadInt64();

        logger.LogInformation(
            "删除留言: AccountId={AccountId}, BulletinId={BulletinId}",
            session.AccountId, bulletinId);

        // TODO: 从数据库删除（需要验证权限）

        await session.SendPacketAsync(Opcodes.SM_BULLETIN, writer =>
        {
            writer.WriteByte((byte)BulletinOperation.Delete);
            writer.WriteBoolean(true); // 成功
            writer.WriteString("留言已删除");
        });
    }

    /// <summary>
    /// 发送错误响应
    /// </summary>
    private async ValueTask SendErrorAsync(ClientSession session, BulletinOperation operation, string message)
    {
        await session.SendPacketAsync(Opcodes.SM_BULLETIN, writer =>
        {
            writer.WriteByte((byte)operation);
            writer.WriteBoolean(false); // 失败
            writer.WriteString(message);
        });
    }
}
