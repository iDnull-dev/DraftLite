using Microsoft.EntityFrameworkCore;
using DraftLite.Data;
using DraftLite.Data.Entities;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;

namespace DraftLite.Service.Services;

public sealed class ProjectService : IProjectService
{
    private readonly DraftLiteDbContext _db;
    private readonly IAppMapper _mapper;

    public ProjectService(DraftLiteDbContext db, IAppMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProjectDto>> ListAsync(string userGoogleId, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);

        var projects = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .Where(p => p.DeletedAt == null && (p.OwnerId == user.Id || p.Collaborators.Any(c => c.UserId == user.Id)))
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);

        return projects.Select(p => _mapper.Map<Project, ProjectDto>(p)).ToList();
    }

    public async Task<ProjectDto> CreateAsync(string userGoogleId, CreateProjectRequest request, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(request));

        var now = DateTimeOffset.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            OwnerId = user.Id,
            Title = title,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        project.Owner = user;
        return _mapper.Map<Project, ProjectDto>(project);
    }

    public async Task<ProjectDto> UpdateAsync(string userGoogleId, Guid projectId, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(request));

        var project = await _db.Projects
            .Include(p => p.Owner)
            .SingleOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null, ct);
        if (project is null) throw new KeyNotFoundException("Project not found.");
        if (project.OwnerId != user.Id) throw new UnauthorizedAccessException("Only owner can update project.");

        project.Title = title;
        project.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<Project, ProjectDto>(project);
    }

    public async Task<Boolean> DeleteAsync(string userGoogleId, Guid projectId, CancellationToken ct = default)
    {
        var user = await RequireUserAsync(userGoogleId, ct);

        var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null, ct);
        if (project is null) return false; 
        if (project.OwnerId != user.Id) throw new UnauthorizedAccessException("Only owner can delete project.");

        project.DeletedAt = DateTimeOffset.UtcNow;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        return await _db.SaveChangesAsync(ct) > 0;
    }

    private async Task<User> RequireUserAsync(string userGoogleId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userGoogleId))
            throw new UnauthorizedAccessException("Missing Google user id.");

        var user = await _db.Users.SingleOrDefaultAsync(u => u.GoogleId == userGoogleId, ct);
        if (user is null) 
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        return user;
    }
}

