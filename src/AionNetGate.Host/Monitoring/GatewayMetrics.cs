using Prometheus;

namespace AionNetGate.Host.Monitoring;

/// <summary>
/// 网关服务 Prometheus 指标收集器
/// </summary>
public static class GatewayMetrics
{
    // 连接指标
    public static readonly Gauge CurrentConnections = Metrics
        .CreateGauge("aiongate_connections_current", "当前活动连接数");

    public static readonly Counter TotalConnections = Metrics
        .CreateCounter("aiongate_connections_total", "累计连接总数");

    public static readonly Counter ConnectionErrors = Metrics
        .CreateCounter("aiongate_connection_errors_total", "连接错误总数");

    // 数据包指标
    public static readonly Counter PacketsReceived = Metrics
        .CreateCounter("aiongate_packets_received_total", "收到的数据包总数",
            new CounterConfiguration { LabelNames = new[] { "opcode" } });

    public static readonly Counter PacketsSent = Metrics
        .CreateCounter("aiongate_packets_sent_total", "发送的数据包总数",
            new CounterConfiguration { LabelNames = new[] { "opcode" } });

    public static readonly Counter PacketErrors = Metrics
        .CreateCounter("aiongate_packet_errors_total", "数据包处理错误总数",
            new CounterConfiguration { LabelNames = new[] { "opcode", "error_type" } });

    // 账号操作指标
    public static readonly Counter LoginAttempts = Metrics
        .CreateCounter("aiongate_login_attempts_total", "登录尝试总数",
            new CounterConfiguration { LabelNames = new[] { "result" } });

    public static readonly Counter RegisterAttempts = Metrics
        .CreateCounter("aiongate_register_attempts_total", "注册尝试总数",
            new CounterConfiguration { LabelNames = new[] { "result" } });

    // 性能指标
    public static readonly Histogram PacketProcessingDuration = Metrics
        .CreateHistogram("aiongate_packet_processing_seconds", "数据包处理耗时（秒）",
            new HistogramConfiguration
            {
                LabelNames = new[] { "opcode" },
                Buckets = new[] { .001, .005, .01, .025, .05, .1, .25, .5, 1, 2.5, 5, 10 }
            });

    // 系统指标
    public static readonly Gauge UptimeSeconds = Metrics
        .CreateGauge("aiongate_uptime_seconds", "服务器运行时间（秒）");

    public static readonly Gauge MemoryUsageBytes = Metrics
        .CreateGauge("aiongate_memory_usage_bytes", "内存使用量（字节）");

    public static readonly Gauge CpuUsagePercent = Metrics
        .CreateGauge("aiongate_cpu_usage_percent", "CPU使用率（百分比）");

    // 数据库指标
    public static readonly Counter DatabaseQueries = Metrics
        .CreateCounter("aiongate_database_queries_total", "数据库查询总数",
            new CounterConfiguration { LabelNames = new[] { "operation" } });

    public static readonly Histogram DatabaseQueryDuration = Metrics
        .CreateHistogram("aiongate_database_query_seconds", "数据库查询耗时（秒）",
            new HistogramConfiguration
            {
                LabelNames = new[] { "operation" },
                Buckets = new[] { .001, .005, .01, .025, .05, .1, .25, .5, 1, 2.5, 5 }
            });

    // IP黑名单指标
    public static readonly Gauge BlacklistedIps = Metrics
        .CreateGauge("aiongate_blacklisted_ips", "黑名单IP数量");

    public static readonly Counter BlockedConnections = Metrics
        .CreateCounter("aiongate_blocked_connections_total", "被阻止的连接总数");
}
