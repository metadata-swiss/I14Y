using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Extensions;

public static class ApplicationBuilderExensions
{
    /// <summary>
    /// Adds headers to all responses related to security scans
    /// </summary>
    public static void UseSecurityHeaders(this IApplicationBuilder appBuilder)
    {
        appBuilder.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                return Task.FromResult(0);
            });

            await next();
        });
    }
}