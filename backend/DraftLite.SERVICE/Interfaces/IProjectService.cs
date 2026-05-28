using DraftLite.DTO;

namespace DraftLite.Service.Interfaces;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> ListAsync(string userGoogleId, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(string userGoogleId, CreateProjectRequest request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(string userGoogleId, Guid projectId, UpdateProjectRequest request, CancellationToken ct = default);
    Task<Boolean> DeleteAsync(string userGoogleId, Guid projectId, CancellationToken ct = default);
}

