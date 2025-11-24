namespace AionNetGate.Core.Domain.ValueObjects;

/// <summary>
/// 邮箱地址值对象 - 不可变、强类型
/// </summary>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    /// <summary>
    /// 邮箱地址值
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 创建邮箱地址实例
    /// </summary>
    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 创建邮箱地址（工厂方法）
    /// </summary>
    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("邮箱地址不能为空", nameof(value));

        if (value.Length > 254)
            throw new ArgumentException("邮箱地址长度不能超过254个字符", nameof(value));

        // RFC 5322 简化版邮箱验证
        if (!System.Text.RegularExpressions.Regex.IsMatch(
            value,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new ArgumentException("邮箱地址格式无效", nameof(value));
        }

        // 规范化为小写
        var normalized = value.ToLowerInvariant();
        return new EmailAddress(normalized);
    }

    /// <summary>
    /// 尝试创建邮箱地址（不抛出异常）
    /// </summary>
    public static bool TryCreate(string? value, out EmailAddress? emailAddress)
    {
        emailAddress = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            emailAddress = Create(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取邮箱域名部分
    /// </summary>
    public string GetDomain()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex >= 0 ? Value.Substring(atIndex + 1) : string.Empty;
    }

    /// <summary>
    /// 获取邮箱本地部分（@之前）
    /// </summary>
    public string GetLocalPart()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex >= 0 ? Value.Substring(0, atIndex) : Value;
    }

    /// <summary>
    /// 值相等性比较
    /// </summary>
    public bool Equals(EmailAddress? other)
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
        return obj is EmailAddress other && Equals(other);
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
    public static implicit operator string(EmailAddress emailAddress) => emailAddress.Value;

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(EmailAddress? left, EmailAddress? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(EmailAddress? left, EmailAddress? right)
    {
        return !(left == right);
    }
}
