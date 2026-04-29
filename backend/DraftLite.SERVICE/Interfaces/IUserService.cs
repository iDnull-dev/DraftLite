using DraftLite.Data.Entities;
using DraftLite.Dto;

namespace DraftLite.Service.Interfaces;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<User> GetMeAsync(String userGoogleId, CancellationToken ct = default);
    Task<string> GetThemeAsync(string userGoogleId, CancellationToken ct = default);

    Task<IReadOnlyList<User>> SearchAsync(string searchName, int limit = 20, CancellationToken ct = default);
    Task<User> UpdateMeAsync(String userGoogleId, UpdateMeRequest request, CancellationToken ct = default);
    Task<User> UpdateThemeAsync(string userGoogleId, UpdateThemeRequest request, CancellationToken ct = default);

    Task<User> AdminUpdateAsync(Guid targetUserId, AdminUpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid targetUserId, CancellationToken ct = default);
}
