using System.Management;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Launcher.Template.Services;

/// <summary>
/// 网关客户端实现
/// </summary>
public class GatewayClient : IGatewayClient
{
    private readonly ILogger<GatewayClient> _logger;
    private Socket? _socket;
    private CancellationTokenSource? _cts;

    public GatewayClient(ILogger<GatewayClient> logger)
    {
        _logger = logger;
    }

    public bool IsConnected => _socket?.Connected ?? false;

    public async Task<bool> ConnectAsync(string host, int port, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("正在连接到网关服务器 {Host}:{Port}", host, port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _cts = new CancellationTokenSource();

            await _socket.ConnectAsync(host, port, ct);

            if (_socket.Connected)
            {
                _logger.LogInformation("成功连接到网关服务器");

                // TODO: 发送 CM_ConnectRequest packet
                // var hardwareId = GetHardwareId();
                // await SendConnectRequestAsync(hardwareId, ct);

                return true;
            }

            _logger.LogWarning("连接到网关服务器失败");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接网关服务器时发生异常");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _cts?.Cancel();

            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }

            _logger.LogInformation("已断开与网关服务器的连接");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接时发生异常");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取硬件ID（CPU + 主板序列号）
    /// </summary>
    private string GetHardwareId()
    {
        try
        {
            var cpuId = GetCpuId();
            var motherboardId = GetMotherboardId();
            return $"{cpuId}-{motherboardId}";
        }
        catch
        {
            return "UNKNOWN";
        }
    }

    private string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? "UNKNOWN";
            }
        }
        catch
        {
            return "UNKNOWN";
        }

        return "UNKNOWN";
    }

    private string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString() ?? "UNKNOWN";
            }
        }
        catch
        {
            return "UNKNOWN";
        }

        return "UNKNOWN";
    }
}
