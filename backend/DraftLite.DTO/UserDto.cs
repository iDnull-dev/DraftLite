namespace DraftLite.Dto;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string Pseudo { get; init; } = null!;
    public string Theme { get; init; } = "light";
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? BanAt { get; init; }
    public string? BanReason { get; init; }
    public string RoleName { get; init; } = null!;
}
