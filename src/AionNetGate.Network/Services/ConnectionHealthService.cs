using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Services;

/// <summary>
/// 连接健康服务 - 跟踪和评估连接质量
/// </summary>
public class ConnectionHealthService
{
    private readonly ILogger<ConnectionHealthService> _logger;
    private readonly ConcurrentDictionary<int, ConnectionHealth> _healthData = new();

    /// <summary>
    /// 延迟样本数量（用于计算平均值）
    /// </summary>
    private const int LatencySampleCount = 10;

    /// <summary>
    /// 健康评分阈值
    /// </summary>
    public const double HealthyThreshold = 0.8;
    public const double WarningThreshold = 0.5;

    public ConnectionHealthService(ILogger<ConnectionHealthService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 记录心跳成功
    /// </summary>
    public void RecordHeartbeat(int sessionId)
    {
        var health = GetOrCreateHealth(sessionId);
        health.HeartbeatCount++;
        health.LastHeartbeat = DateTime.UtcNow;
    }

    /// <summary>
    /// 记录延迟
    /// </summary>
    public void RecordLatency(int sessionId, long latencyMs)
    {
        var health = GetOrCreateHealth(sessionId);
        health.AddLatencySample(latencyMs);
    }

    /// <summary>
    /// 记录超时
    /// </summary>
    public void RecordTimeout(int sessionId)
    {
        var health = GetOrCreateHealth(sessionId);
        health.TimeoutCount++;
    }

    /// <summary>
    /// 记录丢包
    /// </summary>
    public void RecordPacketLoss(int sessionId)
    {
        var health = GetOrCreateHealth(sessionId);
        health.PacketLossCount++;
    }

    /// <summary>
    /// 记录数据包接收
    /// </summary>
    public void RecordPacketReceived(int sessionId, int size)
    {
        var health = GetOrCreateHealth(sessionId);
        health.PacketsReceived++;
        health.BytesReceived += size;
    }

    /// <summary>
    /// 记录数据包发送
    /// </summary>
    public void RecordPacketSent(int sessionId, int size)
    {
        var health = GetOrCreateHealth(sessionId);
        health.PacketsSent++;
        health.BytesSent += size;
    }

    /// <summary>
    /// 获取连接健康信息
    /// </summary>
    public ConnectionHealth? GetHealth(int sessionId)
    {
        return _healthData.TryGetValue(sessionId, out var health) ? health : null;
    }

    /// <summary>
    /// 获取连接健康评分（0-1）
    /// </summary>
    public double GetHealthScore(int sessionId)
    {
        var health = GetHealth(sessionId);
        return health?.CalculateScore() ?? 0;
    }

    /// <summary>
    /// 获取连接状态
    /// </summary>
    public ConnectionStatus GetStatus(int sessionId)
    {
        var score = GetHealthScore(sessionId);
        return score switch
        {
            >= HealthyThreshold => ConnectionStatus.Healthy,
            >= WarningThreshold => ConnectionStatus.Warning,
            > 0 => ConnectionStatus.Critical,
            _ => ConnectionStatus.Unknown
        };
    }

    /// <summary>
    /// 移除连接健康数据
    /// </summary>
    public void RemoveHealth(int sessionId)
    {
        _healthData.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// 获取所有连接的健康摘要
    /// </summary>
    public HealthSummary GetSummary()
    {
        var healths = _healthData.Values.ToList();

        return new HealthSummary
        {
            TotalConnections = healths.Count,
            HealthyCount = healths.Count(h => h.CalculateScore() >= HealthyThreshold),
            WarningCount = healths.Count(h => h.CalculateScore() >= WarningThreshold && h.CalculateScore() < HealthyThreshold),
            CriticalCount = healths.Count(h => h.CalculateScore() < WarningThreshold && h.CalculateScore() > 0),
            AverageLatency = healths.Average(h => h.AverageLatency),
            TotalBytesReceived = healths.Sum(h => h.BytesReceived),
            TotalBytesSent = healths.Sum(h => h.BytesSent)
        };
    }

    private ConnectionHealth GetOrCreateHealth(int sessionId)
    {
        return _healthData.GetOrAdd(sessionId, _ => new ConnectionHealth(sessionId, LatencySampleCount));
    }
}

/// <summary>
/// 单个连接的健康信息
/// </summary>
public class ConnectionHealth
{
    private readonly int _sessionId;
    private readonly Queue<long> _latencySamples;
    private readonly int _maxSamples;
    private readonly object _lock = new();

    public int SessionId => _sessionId;

    /// <summary>
    /// 心跳成功次数
    /// </summary>
    public int HeartbeatCount { get; set; }

    /// <summary>
    /// 超时次数
    /// </summary>
    public int TimeoutCount { get; set; }

    /// <summary>
    /// 丢包次数
    /// </summary>
    public int PacketLossCount { get; set; }

    /// <summary>
    /// 接收的数据包数
    /// </summary>
    public long PacketsReceived { get; set; }

    /// <summary>
    /// 发送的数据包数
    /// </summary>
    public long PacketsSent { get; set; }

    /// <summary>
    /// 接收的字节数
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 发送的字节数
    /// </summary>
    public long BytesSent { get; set; }

    /// <summary>
    /// 上次心跳时间
    /// </summary>
    public DateTime LastHeartbeat { get; set; }

    /// <summary>
    /// 连接建立时间
    /// </summary>
    public DateTime ConnectedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// 平均延迟（毫秒）
    /// </summary>
    public double AverageLatency
    {
        get
        {
            lock (_lock)
            {
                return _latencySamples.Count > 0 ? _latencySamples.Average() : 0;
            }
        }
    }

    /// <summary>
    /// 最小延迟
    /// </summary>
    public long MinLatency
    {
        get
        {
            lock (_lock)
            {
                return _latencySamples.Count > 0 ? _latencySamples.Min() : 0;
            }
        }
    }

    /// <summary>
    /// 最大延迟
    /// </summary>
    public long MaxLatency
    {
        get
        {
            lock (_lock)
            {
                return _latencySamples.Count > 0 ? _latencySamples.Max() : 0;
            }
        }
    }

    public ConnectionHealth(int sessionId, int maxSamples = 10)
    {
        _sessionId = sessionId;
        _maxSamples = maxSamples;
        _latencySamples = new Queue<long>(maxSamples);
        LastHeartbeat = DateTime.UtcNow;
    }

    /// <summary>
    /// 添加延迟样本
    /// </summary>
    public void AddLatencySample(long latencyMs)
    {
        lock (_lock)
        {
            if (_latencySamples.Count >= _maxSamples)
            {
                _latencySamples.Dequeue();
            }
            _latencySamples.Enqueue(latencyMs);
        }
    }

    /// <summary>
    /// 计算健康评分（0-1）
    /// </summary>
    public double CalculateScore()
    {
        var totalAttempts = HeartbeatCount + TimeoutCount;
        if (totalAttempts == 0) return 1.0;

        // 基础分数：心跳成功率
        var successRate = (double)HeartbeatCount / totalAttempts;

        // 延迟惩罚：超过 500ms 开始惩罚
        var latencyPenalty = Math.Min(AverageLatency / 1000.0, 0.3);

        // 丢包惩罚
        var totalPackets = PacketsReceived + PacketsSent;
        var lossRate = totalPackets > 0 ? (double)PacketLossCount / totalPackets : 0;
        var lossPenalty = Math.Min(lossRate * 2, 0.3);

        var score = successRate - latencyPenalty - lossPenalty;
        return Math.Max(0, Math.Min(1, score));
    }

    /// <summary>
    /// 连接时长
    /// </summary>
    public TimeSpan ConnectionDuration => DateTime.UtcNow - ConnectedAt;
}

/// <summary>
/// 连接状态
/// </summary>
public enum ConnectionStatus
{
    Unknown,
    Healthy,
    Warning,
    Critical
}

/// <summary>
/// 健康摘要
/// </summary>
public record HealthSummary
{
    public int TotalConnections { get; init; }
    public int HealthyCount { get; init; }
    public int WarningCount { get; init; }
    public int CriticalCount { get; init; }
    public double AverageLatency { get; init; }
    public long TotalBytesReceived { get; init; }
    public long TotalBytesSent { get; init; }
}
