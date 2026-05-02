using System.Text.Json.Serialization;
namespace MechanicShop.Domain.Common.Results;

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Deleted;
public readonly record struct Updated;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;

    public static Result<TValue> Failure<TValue>(List<Error> errors) => errors;

}

public sealed class Result<T> : IResult<T>
{
    private readonly T? _value;
    private readonly List<Error>? _errors;

    public bool IsSuccess { get; }

    public IReadOnlyList<Error> Errors => !IsSuccess ? _errors! : [];

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public Error? TopError => !IsSuccess && _errors != null && _errors.Count > 0 ? _errors[0] : null;

    [JsonConstructor]
    private Result(bool isSuccess, T value, IReadOnlyList<Error>? errors)
    {
        if (isSuccess)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null for a successful result.");
            }

            IsSuccess = true;
            _value = value;
            return;
        }

        if (errors == null || errors.Count == 0)
        {
            throw new ArgumentException("Errors cannot be null or empty for a failed result.", nameof(errors));
        }

        IsSuccess = false;
        _errors = [.. errors];
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _errors = [error];
    }

    private Result(List<Error> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            throw new ArgumentException("Errors cannot be null or empty.", nameof(errors));
        }

        IsSuccess = false;
        _errors = errors;
    }

    private Result(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        IsSuccess = true;
        _value = value;
    }

    public TNextValue Match<TNextValue>(Func<T, TNextValue> onSuccess, Func<List<Error>, TNextValue> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(_value!) : onFailure(_errors!);
    }

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);

    public static implicit operator Result<T>(List<Error> errors) => new(errors);
}
