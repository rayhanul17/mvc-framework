namespace DynamicRoleMenuSystem.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new List<string>();

    private Result(bool isSuccess, T? data, string? errorMessage = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        if (errors != null)
            Errors = errors;
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data);
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, default, errorMessage);
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, default, errors: errors);
    }

    public static Result<T> Failure(string errorMessage, List<string> errors)
    {
        return new Result<T>(false, default, errorMessage, errors);
    }
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new List<string>();

    private Result(bool isSuccess, string? errorMessage = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        if (errors != null)
            Errors = errors;
    }

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(string errorMessage)
    {
        return new Result(false, errorMessage);
    }

    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors: errors);
    }

    public static Result Failure(string errorMessage, List<string> errors)
    {
        return new Result(false, errorMessage, errors);
    }
}