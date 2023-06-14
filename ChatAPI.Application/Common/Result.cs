using FluentValidation.Results;
using Microsoft.Extensions.Primitives;

namespace ChatAPI.Application.Common;

public interface IAppResult
{
    bool Success { get; }
    IDictionary<string, string[]>? Errors { get; }
    object? GetSuccessData();
}
public sealed class Result : Result<object?>
{
    public Result() { }
    public Result(ValidationResult? ValidationResult) : base(ValidationResult) { }
    public Result(string errorKey, StringValues errors)
    {
        AddError(errorKey, errors);
    }


    public static Result FromError(string key, StringValues errors)
    {
        Result result = new Result();
        result.AddError(key, errors);
        return result;
    }

}
public class Result<TResult> : IAppResult
    where TResult : class?
{
    public TResult? SuccessData { get; protected set; }
    public IDictionary<string, string[]>? Errors { get; protected set; }
    public bool Success { get => Errors is null or { Count: 0 }; }

    public Result() { }

    public Result(TResult? successData)
    {
        this.SuccessData = successData;
    }
    public Result(ValidationResult? ValidationResult)
    {
        Errors = ValidationResult?.ToDictionary();
    }

    public static Result<TResult> FromError(string key, StringValues errors)
    {
        return new Result<TResult>().AddError(key, errors);
    }

    public Result<TResult> AddError(string key, StringValues errors)
    {
        Errors ??= new Dictionary<string, string[]>();
        Errors.Add(key, errors!);

        return this;
    }

    public object? GetSuccessData()
    {
        return this.SuccessData;
    }
}