using DraftLite.DTO.AppSettings;

namespace DraftLite.Api.DependencyInjection;

public static class SettingsInjection
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Secret), "Jwt:Secret is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "Jwt:Issuer is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "Jwt:Audience is required")
            .ValidateOnStart();

        services.AddOptions<DraftLiteSettings>()
            .Bind(configuration.GetSection(DraftLiteSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Default_user_role), "DraftLite:Default_user_role is required")
            .ValidateOnStart();

        return services;
    }
}

