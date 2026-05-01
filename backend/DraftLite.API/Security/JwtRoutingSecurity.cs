using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DraftLite.Dto.AppSettings;

namespace DraftLite.Api.Security;

public static class JwtRoutingSecurity
{
    public static IServiceCollection AddJwtRoutingSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Jwt settings are missing.");

        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be at least 32 chars.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://accounts.google.com";  // Google's OIDC endpoint
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    "https://accounts.google.com",
                    "accounts.google.com"
                },
                ValidateAudience = true,
                ValidAudience = jwt.Audience,  // Your Google Client ID
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    Console.WriteLine($"❌ Auth failed: {ctx.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    Console.WriteLine($"✅ Token valid for: {ctx.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", "Admin");
            });
        });

        return services;
    }
}

