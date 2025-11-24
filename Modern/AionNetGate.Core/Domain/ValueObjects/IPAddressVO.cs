using System.Net;

namespace AionNetGate.Core.Domain.ValueObjects;

/// <summary>
/// IP地址值对象 - 不可变、强类型
/// </summary>
public sealed class IPAddressVO : IEquatable<IPAddressVO>
{
    /// <summary>
    /// 字符串格式的IP地址
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// System.Net.IPAddress 对象
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// 创建IP地址实例
    /// </summary>
    private IPAddressVO(string value, IPAddress ipAddress)
    {
        Value = value;
        IPAddress = ipAddress;
    }

    /// <summary>
    /// 创建IP地址（工厂方法）
    /// </summary>
    public static IPAddressVO Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IP地址不能为空", nameof(value));

        if (!IPAddress.TryParse(value, out var ipAddress))
            throw new ArgumentException($"无效的IP地址格式: {value}", nameof(value));

        return new IPAddressVO(value, ipAddress);
    }

    /// <summary>
    /// 从 IPAddress 对象创建
    /// </summary>
    public static IPAddressVO Create(IPAddress ipAddress)
    {
        if (ipAddress == null)
            throw new ArgumentNullException(nameof(ipAddress));

        var value = ipAddress.ToString();
        return new IPAddressVO(value, ipAddress);
    }

    /// <summary>
    /// 尝试创建IP地址（不抛出异常）
    /// </summary>
    public static bool TryCreate(string? value, out IPAddressVO? ipAddressVO)
    {
        ipAddressVO = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            ipAddressVO = Create(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 判断是否为IPv4地址
    /// </summary>
    public bool IsIPv4()
    {
        return IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    /// <summary>
    /// 判断是否为IPv6地址
    /// </summary>
    public bool IsIPv6()
    {
        return IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// 判断是否为本地回环地址
    /// </summary>
    public bool IsLoopback()
    {
        return System.Net.IPAddress.IsLoopback(IPAddress);
    }

    /// <summary>
    /// 判断是否为私有网络地址
    /// </summary>
    public bool IsPrivate()
    {
        if (!IsIPv4())
            return false;

        var bytes = IPAddress.GetAddressBytes();

        // 10.0.0.0/8
        if (bytes[0] == 10)
            return true;

        // 172.16.0.0/12
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            return true;

        // 192.168.0.0/16
        if (bytes[0] == 192 && bytes[1] == 168)
            return true;

        return false;
    }

    /// <summary>
    /// 获取字节表示
    /// </summary>
    public byte[] GetBytes()
    {
        return IPAddress.GetAddressBytes();
    }

    /// <summary>
    /// 值相等性比较
    /// </summary>
    public bool Equals(IPAddressVO? other)
    {
        if (other is null)
            return false;

        return IPAddress.Equals(other.IPAddress);
    }

    /// <summary>
    /// 对象相等性比较
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is IPAddressVO other && Equals(other);
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode()
    {
        return IPAddress.GetHashCode();
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 隐式转换为字符串
    /// </summary>
    public static implicit operator string(IPAddressVO ipAddressVO) => ipAddressVO.Value;

    /// <summary>
    /// 隐式转换为 IPAddress
    /// </summary>
    public static implicit operator IPAddress(IPAddressVO ipAddressVO) => ipAddressVO.IPAddress;

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(IPAddressVO? left, IPAddressVO? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(IPAddressVO? left, IPAddressVO? right)
    {
        return !(left == right);
    }
}
