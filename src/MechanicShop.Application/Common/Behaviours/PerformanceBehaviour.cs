using System.Diagnostics;
using MechanicShop.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace MechanicShop.Application.Common.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse>
    (ILogger<PerformanceBehaviour<TRequest, TResponse>> logger, IOptions<PerformanceSettings> settings)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > settings.Value.LongRunningRequestThresholdInMs)
            {
                var requestName = typeof(TRequest).Name;
                var traceId = Activity.Current?.TraceId.ToString();

                logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request} {TraceId}",
                    requestName, elapsedMilliseconds, request, traceId);
            }
        }
    }
}