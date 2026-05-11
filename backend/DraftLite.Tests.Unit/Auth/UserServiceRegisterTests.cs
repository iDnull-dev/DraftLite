using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DraftLite.Data;
using DraftLite.Data.Entities;
using DraftLite.DTO;
using DraftLite.DTO.AppSettings;
using DraftLite.Service.Services;
using DraftLite.Tests.Unit.Helpers;

namespace DraftLite.Tests.Unit.Auth;

/// <summary>
/// Unit tests for <see cref="UserService.RegisterAsync"/> — the logic behind <c>POST /users/register</c>.
/// </summary>
public sealed class UserServiceRegisterTests
{
    private static UserService CreateService(DraftLiteDbContext db, string defaultRoleName = "User")
    {
        var settings = Options.Create(new DraftLiteSettings { Default_user_role = defaultRoleName });
        return new UserService(db, settings);
    }

    private static async Task<Role> EnsureRoleAsync(DraftLiteDbContext db, string name)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == name);
        if (role is not null)
            return role;

        role = new Role { Name = name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserWithDefaultRole()
    {
        var db = DbContextFactory.Create();
        await EnsureRoleAsync(db, "User");
        var service = CreateService(db);

        var result = await service.RegisterAsync(new RegisterUserRequest
        {
            Email = "  new@example.com  ",
            Pseudo = "  player1  ",
            GoogleId = "gid-001"
        });

        db.Users.Should().HaveCount(1);
        result.Email.Should().Be("new@example.com");
        result.Pseudo.Should().Be("player1");
        result.GoogleId.Should().Be("gid-001");
        result.Role.Name.Should().Be("User");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_UpdatesPseudoAndOptionalGoogleId()
    {
        var db = DbContextFactory.Create();
        await EnsureRoleAsync(db, "User");
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterUserRequest
        {
            Email = "same@example.com",
            Pseudo = "old",
            GoogleId = null
        });

        var result = await service.RegisterAsync(new RegisterUserRequest
        {
            Email = "same@example.com",
            Pseudo = "new-pseudo",
            GoogleId = "google-linked"
        });

        db.Users.Should().HaveCount(1);
        result.Pseudo.Should().Be("new-pseudo");
        result.GoogleId.Should().Be("google-linked");
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ThrowsArgumentException()
    {
        var db = DbContextFactory.Create();
        await EnsureRoleAsync(db, "User");
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterUserRequest
            {
                Email = "   ",
                Pseudo = "x"
            }));
    }

    [Fact]
    public async Task RegisterAsync_EmptyPseudo_ThrowsArgumentException()
    {
        var db = DbContextFactory.Create();
        await EnsureRoleAsync(db, "User");
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterUserRequest
            {
                Email = "a@b.com",
                Pseudo = "  "
            }));
    }

    [Fact]
    public async Task RegisterAsync_NoRolesSeeded_ThrowsInvalidOperationException()
    {
        var db = DbContextFactory.Create();
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterUserRequest
            {
                Email = "orphan@example.com",
                Pseudo = "orphan"
            }));
    }

    [Fact]
    public async Task RegisterAsync_DefaultRoleMissing_ThrowsInvalidOperationException()
    {
        var db = DbContextFactory.Create();
        await EnsureRoleAsync(db, "Admin");
        var service = CreateService(db, defaultRoleName: "User");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterUserRequest
            {
                Email = "x@y.com",
                Pseudo = "x"
            }));
    }
}
