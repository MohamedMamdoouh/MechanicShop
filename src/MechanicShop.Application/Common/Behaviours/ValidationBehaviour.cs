using FluentValidation;
using MechanicShop.Domain.Common.Results;
using MediatR;
namespace MechanicShop.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(
    IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next(cancellationToken);
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next(cancellationToken);
        }

        var errors = validationResult.Errors
            .ConvertAll(error => Error.Validation(
                description: error.ErrorMessage,
                code: error.ErrorCode));

        return ResultResponseFactory.Failure<TResponse>(errors);
    }
}