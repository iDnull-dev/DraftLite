using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using DraftLite.DTO.AppSettings;

namespace DraftLite.Api.Security;

public static class JwtRoutingSecurity
{
    public const string IntegrationTestEnvironmentName = "IntegrationTest";

    public static IServiceCollection AddJwtRoutingSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Jwt settings are missing.");

        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be at least 32 chars.");

        if (environment.IsEnvironment(IntegrationTestEnvironmentName))
            AddSymmetricJwtBearer(services, jwt);
        else
            AddGoogleJwtBearer(services, jwt);

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

    /// <summary>
    /// HS256 validation aligned with <see cref="DraftLite.Service.Services.JwtService"/> (integration / local API tokens).
    /// </summary>
    private static void AddSymmetricJwtBearer(IServiceCollection services, JwtSettings jwt)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    /// <summary>Validate Google-issued ID tokens (production default).</summary>
    private static void AddGoogleJwtBearer(IServiceCollection services, JwtSettings jwt)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://accounts.google.com";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[]
                    {
                        "https://accounts.google.com",
                        "accounts.google.com"
                    },
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
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
    }
}
