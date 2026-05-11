using Microsoft.EntityFrameworkCore;
using DraftLite.Data;
using DraftLite.Data.Entities;

namespace DraftLite.Tests.Unit.Helpers;

/// <summary>
/// Creates a fresh in-memory DraftLiteDbContext for each test.
/// Each call returns an isolated DB — tests never share state.
/// </summary>
public static class DbContextFactory
{
    public static DraftLiteDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<DraftLiteDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString()) // unique name = isolated DB
            .Options;

        var context = new DraftLiteDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    // ── Pre-built seed helpers ───────────────────────────────────────────────

    /// <summary>Returns a standard active user already saved to the DB.</summary>
    public static async Task<User> SeedActiveUserAsync(DraftLiteDbContext db,
        string googleId = "google-123",
        string email = "lucas@example.com",
        string pseudo = "lucas")
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "User")
                   ?? db.Roles.Add(new Role { Name = "User" }).Entity;
        await db.SaveChangesAsync();

        var user = new User
        {
            GoogleId = googleId,
            Email = email,
            Pseudo = pseudo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            RoleId = role.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    /// <summary>Returns a banned user already saved to the DB.</summary>
    public static async Task<User> SeedBannedUserAsync(DraftLiteDbContext db,
        string googleId = "google-banned",
        string email = "banned@example.com",
        string banReason = "Terms of service violation")
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "User")
                   ?? db.Roles.Add(new Role { Name = "User" }).Entity;
        await db.SaveChangesAsync();

        var user = new User
        {
            GoogleId = googleId,
            Email = email,
            Pseudo = "banned-user",
            IsActive = false,
            BanAt = DateTime.UtcNow.AddDays(-1),
            BanReason = banReason,
            CreatedAt = DateTime.UtcNow,
            RoleId = role.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
