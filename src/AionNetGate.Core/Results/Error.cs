namespace AionNetGate.Core.Results;

/// <summary>
/// 表示错误信息的记录类型
/// </summary>
/// <param name="Code">错误代码</param>
/// <param name="Message">错误消息</param>
public record Error(string Code, string Message)
{
    /// <summary>
    /// 表示没有错误
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// 表示资源未找到错误
    /// </summary>
    public static Error NotFound(string message) => new("NotFound", message);

    /// <summary>
    /// 表示验证错误
    /// </summary>
    public static Error Validation(string message) => new("Validation", message);

    /// <summary>
    /// 表示冲突错误（如用户名已存在）
    /// </summary>
    public static Error Conflict(string message) => new("Conflict", message);

    /// <summary>
    /// 表示未授权错误
    /// </summary>
    public static Error Unauthorized(string message) => new("Unauthorized", message);

    /// <summary>
    /// 表示禁止访问错误
    /// </summary>
    public static Error Forbidden(string message) => new("Forbidden", message);

    /// <summary>
    /// 表示内部错误
    /// </summary>
    public static Error Internal(string message) => new("Internal", message);

    /// <summary>
    /// 表示服务不可用错误
    /// </summary>
    public static Error ServiceUnavailable(string message) => new("ServiceUnavailable", message);

    /// <summary>
    /// 表示操作已取消错误
    /// </summary>
    public static Error Cancelled(string message) => new("Cancelled", message);

    /// <summary>
    /// 表示超时错误
    /// </summary>
    public static Error Timeout(string message) => new("Timeout", message);
}
