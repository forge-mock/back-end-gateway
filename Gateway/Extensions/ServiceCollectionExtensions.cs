using Shared.Interfaces;
using Shared.Services;

namespace Gateway.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void AddApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IMiddlewareService, MiddlewareService>();
    }
}