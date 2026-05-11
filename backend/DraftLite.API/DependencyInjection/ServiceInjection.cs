using DraftLite.Service.Interfaces;
using DraftLite.Service.Services;
using DraftLite.Api.Mapping;

namespace DraftLite.Api.DependencyInjection;

public static class ServiceInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // AutoMapper: scan mapping profiles from this assembly
        services.AddAutoMapper(typeof(AppMappingProfile).Assembly);

        // Bridge existing abstraction to AutoMapper
        services.AddScoped<IAppMapper, AppMapper>();

        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtService>();

        return services;
    }
}

