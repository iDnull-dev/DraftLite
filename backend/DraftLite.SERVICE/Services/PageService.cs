using Microsoft.EntityFrameworkCore;
using DraftLite.Data;
using DraftLite.Data.Entities;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;

namespace DraftLite.Service.Services;

public sealed class PageService : IPageService
{
    private readonly DraftLiteDbContext _db;

    public PageService(DraftLiteDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PageDto>> ListByProjectAsync(string userGoogleId, Guid projectId, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        await EnsureProjectAccessAsync(projectId, user.Id, ct);

        var pages = await _db.Pages
            .Where(p => p.ProjectId == projectId && p.DeletedAt == null)
            .OrderBy(p => p.OrderIndex)
            .ToListAsync(ct);

        return pages.Select(Map).ToList();
    }

    public async Task<PageDto> CreateAsync(string userGoogleId, Guid projectId, CreatePageRequest request, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        await EnsureProjectAccessAsync(projectId, user.Id, ct);

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(request));

        var nextOrder = await _db.Pages
            .Where(p => p.ProjectId == projectId && p.DeletedAt == null)
            .Select(p => (int?)p.OrderIndex)
            .MaxAsync(ct) ?? 0;

        var page = new Page
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title,
            Blocks = "[]",
            OrderIndex = nextOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Pages.Add(page);
        await _db.SaveChangesAsync(ct);

        return Map(page);
    }

    public async Task<PageDto> UpdateAsync(string userGoogleId, Guid projectId, Guid pageId, UpdatePageRequest request, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        await EnsureProjectAccessAsync(projectId, user.Id, ct);

        var page = await _db.Pages.SingleOrDefaultAsync(p => p.Id == pageId && p.ProjectId == projectId && p.DeletedAt == null, ct);
        if (page is null) throw new KeyNotFoundException("Page not found.");

        var title = request.Title.Trim();
        if (!string.IsNullOrWhiteSpace(title)) page.Title = title;
        page.Blocks = string.IsNullOrWhiteSpace(request.Blocks) ? "[]" : request.Blocks;
        page.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Map(page);
    }

    public async Task DeleteAsync(string userGoogleId, Guid projectId, Guid pageId, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        await EnsureProjectAccessAsync(projectId, user.Id, ct);

        var page = await _db.Pages.SingleOrDefaultAsync(p => p.Id == pageId && p.ProjectId == projectId && p.DeletedAt == null, ct);
        if (page is null) return;

        page.DeletedAt = DateTimeOffset.UtcNow;
        page.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<User> RequireUserAsync(string userGoogleId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userGoogleId))
            throw new UnauthorizedAccessException("Missing Google user id.");

        var user = await _db.Users.SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);
        if (user is null) throw new KeyNotFoundException("User not found.");
        return user;
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId, CancellationToken ct)
    {
        var hasAccess = await _db.Projects
            .Include(p => p.Collaborators)
            .AnyAsync(p => p.Id == projectId
                && p.DeletedAt == null
                && (p.OwnerId == userId || p.Collaborators.Any(c => c.UserId == userId)), ct);

        if (!hasAccess) throw new UnauthorizedAccessException("No access to this project.");
    }

    private static PageDto Map(Page page) => new()
    {
        Id = page.Id,
        ProjectId = page.ProjectId,
        Title = page.Title,
        Blocks = page.Blocks,
        OrderIndex = page.OrderIndex,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        DeletedAt = page.DeletedAt
    };
}

