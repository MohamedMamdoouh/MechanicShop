using System.Diagnostics;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Common.Behaviours;

public sealed class TransactionBehaviour<TRequest, TResponse>(
    ILogger<TransactionBehaviour<TRequest, TResponse>> logger,
    IAppDbContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ICachedQuery)
            return next(cancellationToken);

        return context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await next(cancellationToken);

                if (result.IsSuccess)
                    await transaction.CommitAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(cancellationToken);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogWarning(
                    ex,
                    "Concurrency conflict for Request {Name} {TraceId}",
                    typeof(TRequest).Name,
                    Activity.Current?.TraceId.ToString());

                return ResultResponseFactory.Failure<TResponse>(ApplicationErrors.ConcurrencyConflict);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogError(
                    ex,
                    "Database error for Request {Name} {TraceId}",
                    typeof(TRequest).Name,
                    Activity.Current?.TraceId.ToString());

                return ResultResponseFactory.Failure<TResponse>(ApplicationErrors.DatabaseError);
            }
        });
    }
}
