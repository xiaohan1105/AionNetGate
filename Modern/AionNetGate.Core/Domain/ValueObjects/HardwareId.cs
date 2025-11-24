namespace AionNetGate.Core.Domain.ValueObjects;

/// <summary>
/// 硬件ID值对象 - 不可变、强类型
/// </summary>
public sealed class HardwareId : IEquatable<HardwareId>
{
    /// <summary>
    /// 硬件ID值
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 创建硬件ID实例
    /// </summary>
    private HardwareId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 创建硬件ID（工厂方法）
    /// </summary>
    public static HardwareId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("硬件ID不能为空", nameof(value));

        if (value.Length < 8 || value.Length > 128)
            throw new ArgumentException("硬件ID长度必须在8到128个字符之间", nameof(value));

        // 硬件ID应该是字母数字组合（可能包含短横线）
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9\-]+$"))
            throw new ArgumentException("硬件ID只能包含字母、数字和短横线", nameof(value));

        return new HardwareId(value);
    }

    /// <summary>
    /// 尝试创建硬件ID（不抛出异常）
    /// </summary>
    public static bool TryCreate(string? value, out HardwareId? hardwareId)
    {
        hardwareId = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            hardwareId = Create(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 值相等性比较
    /// </summary>
    public bool Equals(HardwareId? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 对象相等性比较
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is HardwareId other && Equals(other);
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 隐式转换为字符串
    /// </summary>
    public static implicit operator string(HardwareId hardwareId) => hardwareId.Value;

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(HardwareId? left, HardwareId? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(HardwareId? left, HardwareId? right)
    {
        return !(left == right);
    }
}
