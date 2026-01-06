namespace AionNetGate.Core.Results;

/// <summary>
/// 表示操作结果（无返回值）
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// 指示操作是否成功
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 指示操作是否失败
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 操作失败时的错误信息
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// 根据条件创建结果
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);
}

/// <summary>
/// 表示带返回值的操作结果
/// </summary>
/// <typeparam name="TValue">返回值类型</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// 操作成功时的返回值
    /// </summary>
    /// <exception cref="InvalidOperationException">当操作失败时访问此属性会抛出异常</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    /// 尝试获取返回值
    /// </summary>
    public bool TryGetValue(out TValue? value)
    {
        value = _value;
        return IsSuccess;
    }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(true, value, Error.None);

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static new Result<TValue> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// 根据条件创建结果
    /// </summary>
    public static Result<TValue> Create(TValue? value, Error error) =>
        value is not null ? Success(value) : Failure(error);
}
