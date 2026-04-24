using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Common.Behaviours;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    IUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? "Anonymous";
        var traceId = Activity.Current?.TraceId.ToString();

        logger.LogInformation(
            "Handling request {RequestName} for user {UserId} with trace {TraceId}",
            requestName, userId, traceId);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            sw.Stop();
            logger.LogInformation(
                "{RequestName} completed in {ElapsedMs}ms | {TraceId}",
                requestName, sw.ElapsedMilliseconds, traceId);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "{RequestName} failed in {ElapsedMs}ms | {TraceId}",
                requestName, sw.ElapsedMilliseconds, traceId);
            throw;
        }
    }
}