using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DraftLite.Api.Security;
using DraftLite.Data;
using DraftLite.Data.Entities;

namespace DraftLite.Tests.Integration;

/// <summary>
/// Hôte API en environnement <see cref="JwtRoutingSecurity.IntegrationTestEnvironmentName"/> :
/// EF InMemory, JWT symétrique (aligné sur les constantes de tests ci-dessous).
/// </summary>
public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Doit rester aligné avec <c>DraftLite.Tests.Unit.Helpers.JwtTestHelper</c>.</summary>
    public const string TestJwtSecret = "super-secret-test-key-32chars-ok!";
    public const string TestJwtIssuer = "DraftLite-api-test";
    public const string TestJwtAudience = "DraftLite-app-test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(JwtRoutingSecurity.IntegrationTestEnvironmentName);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestJwtSecret,
                ["Jwt:Issuer"] = TestJwtIssuer,
                ["Jwt:Audience"] = TestJwtAudience,
                ["Jwt:ExpiryHours"] = "1",
                ["DraftLite:Default_user_role"] = "User",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused_integration"
            });
        });
    }

    /// <summary>Le rôle attendu par <c>UserService.RegisterAsync</c> doit exister avant les appels HTTP.</summary>
    public void SeedUserRole()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DraftLiteDbContext>();
        if (db.Roles.Any(r => r.Name == "User"))
            return;

        db.Roles.Add(new Role { Name = "User" });
        db.SaveChanges();
    }
}
