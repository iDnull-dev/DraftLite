using DraftLite.DTO;

namespace DraftLite.Service.Interfaces;

public interface IPageService
{
    Task<IReadOnlyList<PageDto>> ListByProjectAsync(string userGoogleId, Guid projectId, CancellationToken ct = default);
    Task<PageDto> CreateAsync(string userGoogleId, Guid projectId, CreatePageRequest request, CancellationToken ct = default);
    Task<PageDto> UpdateAsync(string userGoogleId, Guid projectId, Guid pageId, UpdatePageRequest request, CancellationToken ct = default);
    Task DeleteAsync(string userGoogleId, Guid projectId, Guid pageId, CancellationToken ct = default);
}

