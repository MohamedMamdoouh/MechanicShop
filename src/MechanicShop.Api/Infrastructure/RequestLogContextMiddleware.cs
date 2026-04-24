using Serilog.Context;
namespace MechanicShop.Api.Infrastructure;

public class RequestLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("UserIp", context.Connection.RemoteIpAddress))
        using (LogContext.PushProperty("Path", context.Request.Path))
        {
            await next(context);
        }
    }
}