using AionNetGate.Core.Common;
using AionNetGate.Core.Domain;
using AionNetGate.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Application.Queries;

/// <summary>
/// 获取会话Query
/// </summary>
public class GetSessionQuery : IRequest<Result<Session>>
{
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// 获取会话Query处理器
/// </summary>
public class GetSessionQueryHandler : IRequestHandler<GetSessionQuery, Result<Session>>
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<GetSessionQueryHandler> _logger;

    public GetSessionQueryHandler(
        ISessionManager sessionManager,
        ILogger<GetSessionQueryHandler> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Session>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = _sessionManager.GetSessionBySessionId(request.SessionId);

        if (session == null)
        {
            _logger.LogWarning("未找到会话: SessionId={SessionId}", request.SessionId);
            return Result<Session>.Failure("会话不存在");
        }

        if (session.IsExpired(30))
        {
            _logger.LogWarning("会话已过期: SessionId={SessionId}", request.SessionId);
            _sessionManager.RemoveSession(request.SessionId);
            return Result<Session>.Failure("会话已过期");
        }

        return Result<Session>.Success(session);
    }
}

/// <summary>
/// 获取所有在线会话Query
/// </summary>
public class GetAllSessionsQuery : IRequest<Result<IReadOnlyCollection<Session>>>
{
}

/// <summary>
/// 获取所有在线会话Query处理器
/// </summary>
public class GetAllSessionsQueryHandler : IRequestHandler<GetAllSessionsQuery, Result<IReadOnlyCollection<Session>>>
{
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<GetAllSessionsQueryHandler> _logger;

    public GetAllSessionsQueryHandler(
        ISessionManager sessionManager,
        ILogger<GetAllSessionsQueryHandler> logger)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IReadOnlyCollection<Session>>> Handle(
        GetAllSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = _sessionManager.GetAllSessions();

        _logger.LogInformation("查询所有在线会话: Count={Count}", sessions.Count);

        return Result<IReadOnlyCollection<Session>>.Success(sessions);
    }
}
