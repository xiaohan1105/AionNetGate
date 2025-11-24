namespace AionNetGate.Core.Common;

/// <summary>
/// 通用操作结果类型（无返回值）
/// </summary>
public class Result
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 操作是否失败
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 错误消息（失败时）
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// 创建结果实例
    /// </summary>
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("成功的结果不应包含错误消息");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("失败的结果必须包含错误消息");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// 组合多个结果（所有成功才成功）
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Success();
    }
}

/// <summary>
/// 通用操作结果类型（带返回值）
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// 返回值（成功时）
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// 创建结果实例
    /// </summary>
    protected Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        if (isSuccess && value == null)
            throw new InvalidOperationException("成功的结果必须包含返回值");

        Value = value;
    }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public new static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }

    /// <summary>
    /// 隐式转换为泛型结果
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}
