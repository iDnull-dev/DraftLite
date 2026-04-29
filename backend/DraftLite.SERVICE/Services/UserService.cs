using DraftLite.Data;
using DraftLite.Data.Entities;
using DraftLite.Dto;
using DraftLite.Service.Interfaces;
using DraftLite.Dto.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace DraftLite.Service.Services;

public sealed class UserService : IUserService
{
    private readonly DraftLiteDbContext _db;
    private readonly IOptions<DraftLiteSettings> _settings;

    public UserService(DraftLiteDbContext db, IOptions<DraftLiteSettings> settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<User> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var pseudo = request.Pseudo.Trim();

        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(pseudo)) throw new ArgumentException("Pseudo is required.", nameof(request));

        var existing = await _db.Users.Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == email, ct);

        if (existing is not null)
        {
            existing.Pseudo = pseudo;
            if (!string.IsNullOrWhiteSpace(request.GoogleId))
                existing.GoogleId = request.GoogleId.Trim();

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        var defaultRole = await _db.Roles.Where(r => r.Name == _settings.Value.Default_user_role).FirstOrDefaultAsync(ct);

        if (defaultRole is null)
            throw new InvalidOperationException("No roles exist in DB. Seed `role` table first.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Pseudo = pseudo,
            Theme = "light",
            GoogleId = string.IsNullOrWhiteSpace(request.GoogleId) ? null : request.GoogleId.Trim(),
            RoleId = defaultRole.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        user.Role = defaultRole;
        return user;
    }

     public async Task<User> GetMeAsync(string userGoogleId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);

        if (user is null) throw new KeyNotFoundException($"User {userGoogleId} not found.");
        return user;
    }

    public async Task<string> GetThemeAsync(string userGoogleId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);

        if (user is null) throw new KeyNotFoundException($"User {userGoogleId} not found.");
        return user.Theme;
    }

    public async Task<IReadOnlyList<User>> SearchAsync(string searchName, int limit = 20, CancellationToken ct = default)
    {
        var query = (searchName ?? string.Empty).Trim();
        if (query.Length == 0) return Array.Empty<User>();

        limit = Math.Clamp(limit, 1, 50);

        var users = await _db.Users
            .Include(u => u.Role)
            .Where(u => EF.Functions.ILike(u.Pseudo, $"%{query}%"))
            .OrderBy(u => u.Pseudo)
            .Take(limit)
            .ToListAsync(ct);

        return users.Select(u => u).ToList();
    }

    public async Task<User> UpdateMeAsync(String userGoogleId, UpdateMeRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var pseudo = request.Pseudo.Trim();

        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(pseudo)) throw new ArgumentException("Pseudo is required.", nameof(request));

        var user = await _db.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);

        if (user is null) throw new KeyNotFoundException("User not found.");

        user.Email = email;
        user.Pseudo = pseudo;

        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User> AdminUpdateAsync(Guid targetUserId, AdminUpdateUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var pseudo = request.Pseudo.Trim();

        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(pseudo)) throw new ArgumentException("Pseudo is required.", nameof(request));

        var user = await _db.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == targetUserId, ct);

        if (user is null) throw new KeyNotFoundException("User not found.");

        var role = await _db.Roles.SingleOrDefaultAsync(r => r.Id == request.RoleId, ct);
        if (role is null) throw new KeyNotFoundException("Role not found.");

        user.Email = email;
        user.Pseudo = pseudo;
        user.IsActive = request.IsActive;
        user.BanAt = request.BanAt;
        user.BanReason = request.BanReason;
        user.RoleId = role.Id;
        user.Role = role;

        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task DeleteAsync(Guid targetUserId, CancellationToken ct = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (user is null) return;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<User> UpdateThemeAsync(string userGoogleId, UpdateThemeRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);

        if (user is null) throw new KeyNotFoundException($"User {userGoogleId} not found.");

        var theme = (request.Theme ?? "light").Trim().ToLowerInvariant();
        if (theme is not ("light" or "dark"))
            throw new ArgumentException("Theme must be 'light' or 'dark'.", nameof(request));

        user.Theme = theme;
        await _db.SaveChangesAsync(ct);
        return user;
    }
}
