namespace AuthServer.Application.Common;

/// <summary>
///  REpresents the result of an operation that can succeeed or fail.
/// Provides type-safe error handling without throwing exceptions.
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error.");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(string error) => new Result(false, error);

    public static Result<T> Success<T>(T value) => new Result<T>(value, true, null);
    public static Result<T> Failure<T>(string error) => new Result<T>(default, false, error);
}

/// <summary>
/// REpresents the result of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        Value = value;
    }
}
