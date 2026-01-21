using System.Threading.RateLimiting;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

public class RateLimiterDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (ctx, token) =>
            {
                ctx.HttpContext.Response.ContentType = "application/json";

                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers["Retry-After"] = 
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                await ctx.HttpContext.Response.WriteAsJsonAsync(
                    new StandardApiResponse
                    {
                        Status = Status.Warning,
                        Message = "Too many requests. Please slow down."
                    },
                    cancellationToken: token
                );
            };
    
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
            
            options.AddPolicy("AuthTight", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                );
            });

            options.AddPolicy("CodeExecution", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey: key, s =>
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        SegmentsPerWindow = 1,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy("Assistant", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetTokenBucketLimiter(partitionKey: key, _ => new TokenBucketRateLimiterOptions()
                {
                    TokenLimit = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                    TokensPerPeriod = 1,
                    QueueLimit = 10,
                    AutoReplenishment = true
                });
            });
        });
    }
}