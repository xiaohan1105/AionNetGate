using AionNetGate.Core.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AionNetGate.Core.Services;

/// <summary>
/// 会话管理器
/// 负责管理所有活动用户会话的生命周期
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<string, Session> _sessionsBySessionId;
    private readonly ConcurrentDictionary<int, Session> _sessionsByAccountId;
    private readonly ConcurrentDictionary<string, Session> _sessionsByConnectionId;
    private readonly ILogger<SessionManager> _logger;
    private readonly Timer _cleanupTimer;

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionsBySessionId = new ConcurrentDictionary<string, Session>();
        _sessionsByAccountId = new ConcurrentDictionary<int, Session>();
        _sessionsByConnectionId = new ConcurrentDictionary<string, Session>();

        // 启动定时清理过期会话 (每分钟)
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    public Session CreateSession(
        int accountId,
        string accountName,
        string connectionId,
        string ipAddress,
        string? hardwareId = null)
    {
        // 检查是否已有此账号的会话
        if (_sessionsByAccountId.TryGetValue(accountId, out var existingSession))
        {
            _logger.LogWarning(
                "账号已存在活动会话，将移除旧会话: AccountId={AccountId}, OldSessionId={OldSessionId}",
                accountId, existingSession.SessionId);

            RemoveSession(existingSession.SessionId);
        }

        // 创建新会话
        var session = Session.Create(accountId, accountName, connectionId, ipAddress, hardwareId);

        // 添加到所有索引
        _sessionsBySessionId[session.SessionId] = session;
        _sessionsByAccountId[session.AccountId] = session;
        _sessionsByConnectionId[session.ConnectionId] = session;

        _logger.LogInformation(
            "创建新会话: SessionId={SessionId}, AccountId={AccountId}, AccountName={AccountName}, IP={IP}",
            session.SessionId, session.AccountId, session.AccountName, session.IpAddress);

        return session;
    }

    /// <summary>
    /// 通过SessionId获取会话
    /// </summary>
    public Session? GetSessionBySessionId(string sessionId)
    {
        _sessionsBySessionId.TryGetValue(sessionId, out var session);
        return session;
    }

    /// <summary>
    /// 通过AccountId获取会话
    /// </summary>
    public Session? GetSessionByAccountId(int accountId)
    {
        _sessionsByAccountId.TryGetValue(accountId, out var session);
        return session;
    }

    /// <summary>
    /// 通过ConnectionId获取会话
    /// </summary>
    public Session? GetSessionByConnectionId(string connectionId)
    {
        _sessionsByConnectionId.TryGetValue(connectionId, out var session);
        return session;
    }

    /// <summary>
    /// 移除会话
    /// </summary>
    public bool RemoveSession(string sessionId)
    {
        if (!_sessionsBySessionId.TryRemove(sessionId, out var session))
        {
            return false;
        }

        // 从所有索引中移除
        _sessionsByAccountId.TryRemove(session.AccountId, out _);
        _sessionsByConnectionId.TryRemove(session.ConnectionId, out _);

        _logger.LogInformation(
            "移除会话: SessionId={SessionId}, AccountId={AccountId}, Duration={Duration}s",
            session.SessionId, session.AccountId,
            (DateTime.UtcNow - session.CreatedAt).TotalSeconds);

        return true;
    }

    /// <summary>
    /// 通过ConnectionId移除会话
    /// </summary>
    public bool RemoveSessionByConnectionId(string connectionId)
    {
        if (_sessionsByConnectionId.TryGetValue(connectionId, out var session))
        {
            return RemoveSession(session.SessionId);
        }
        return false;
    }

    /// <summary>
    /// 更新会话活动时间
    /// </summary>
    public void UpdateActivity(string sessionId)
    {
        if (_sessionsBySessionId.TryGetValue(sessionId, out var session))
        {
            session.UpdateActivity();
        }
    }

    /// <summary>
    /// 获取所有活动会话
    /// </summary>
    public IReadOnlyCollection<Session> GetAllSessions()
    {
        return _sessionsBySessionId.Values.ToList();
    }

    /// <summary>
    /// 获取活动会话数量
    /// </summary>
    public int GetSessionCount()
    {
        return _sessionsBySessionId.Count;
    }

    /// <summary>
    /// 检查账号是否在线
    /// </summary>
    public bool IsAccountOnline(int accountId)
    {
        return _sessionsByAccountId.ContainsKey(accountId);
    }

    /// <summary>
    /// 定时清理过期会话
    /// </summary>
    private void CleanupExpiredSessions(object? state)
    {
        try
        {
            var expiredSessions = _sessionsBySessionId.Values
                .Where(s => s.IsExpired(30)) // 30分钟超时
                .ToList();

            foreach (var session in expiredSessions)
            {
                _logger.LogInformation(
                    "清理过期会话: SessionId={SessionId}, AccountId={AccountId}",
                    session.SessionId, session.AccountId);

                RemoveSession(session.SessionId);
            }

            if (expiredSessions.Count > 0)
            {
                _logger.LogInformation("已清理 {Count} 个过期会话", expiredSessions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期会话时发生错误");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _sessionsBySessionId.Clear();
        _sessionsByAccountId.Clear();
        _sessionsByConnectionId.Clear();
    }
}

/// <summary>
/// 会话管理器接口
/// </summary>
public interface ISessionManager : IDisposable
{
    Session CreateSession(int accountId, string accountName, string connectionId, string ipAddress, string? hardwareId = null);
    Session? GetSessionBySessionId(string sessionId);
    Session? GetSessionByAccountId(int accountId);
    Session? GetSessionByConnectionId(string connectionId);
    bool RemoveSession(string sessionId);
    bool RemoveSessionByConnectionId(string connectionId);
    void UpdateActivity(string sessionId);
    IReadOnlyCollection<Session> GetAllSessions();
    int GetSessionCount();
    bool IsAccountOnline(int accountId);
}
