namespace DraftLite.DTO;

public sealed class RegisterUserRequest
{
    public string Email { get; init; } = null!;
    public string Pseudo { get; init; } = null!;
    public string? GoogleId { get; init; }
}

public sealed class UpdateMeRequest
{
    public string Email { get; init; } = null!;
    public string Pseudo { get; init; } = null!;
}

public sealed class UpdateThemeRequest
{
    public string Theme { get; init; } = "light";
}

public sealed class AdminUpdateUserRequest
{
    public string Email { get; init; } = null!;
    public string Pseudo { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTimeOffset? BanAt { get; init; }
    public string? BanReason { get; init; }
    public Guid RoleId { get; init; }
}
