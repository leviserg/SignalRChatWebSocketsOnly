using Server.Middleware;

namespace Server.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomWebSocketProtocol(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<CustomWebSocketMiddleware>();

            return applicationBuilder;
        }

    }
}
