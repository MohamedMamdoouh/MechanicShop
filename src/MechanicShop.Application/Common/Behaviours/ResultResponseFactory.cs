using System.Collections.Concurrent;
using System.Reflection;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Application.Common.Behaviours;

internal static class ResultResponseFactory
{
    private static readonly MethodInfo FailureMethodDefinition = typeof(Result)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method =>
            method.Name == nameof(Result.Failure)
            && method.IsGenericMethodDefinition
            && method.GetParameters() is [{ ParameterType: var parameterType }]
            && parameterType == typeof(List<Error>));

    private static readonly ConcurrentDictionary<Type, Func<List<Error>, object>> FailureFactories = new();

    public static TResponse Failure<TResponse>(Error error)
        where TResponse : IResult
        => Failure<TResponse>([error]);

    public static TResponse Failure<TResponse>(List<Error> errors)
        where TResponse : IResult
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        var factory = FailureFactories.GetOrAdd(typeof(TResponse), CreateFactory);
        return (TResponse)factory(errors);
    }

    private static Func<List<Error>, object> CreateFactory(Type responseType)
    {
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Unsupported response type '{responseType}'. Expected Result<T>.");
        }

        var valueType = responseType.GetGenericArguments()[0];
        var failureMethod = FailureMethodDefinition.MakeGenericMethod(valueType);

        return errors => failureMethod.Invoke(null, [errors])!;
    }
}