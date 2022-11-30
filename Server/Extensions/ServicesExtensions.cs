using Server.Services;

namespace Server.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCustomWebSocketProtocol(this IServiceCollection services)
        {
            services.AddSingleton<CustomConnectionManager>();

            return services;
        }
    }
}
