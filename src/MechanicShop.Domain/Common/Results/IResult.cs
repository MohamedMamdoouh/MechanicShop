namespace MechanicShop.Domain.Common.Results;

public interface IResult
{
    bool IsSuccess { get; }

    IReadOnlyList<Error> Errors { get; }
}

public interface IResult<out T> : IResult
{
    T? Value { get; }
}