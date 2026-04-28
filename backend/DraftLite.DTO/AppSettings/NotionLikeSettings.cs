namespace NotionLike.Dto.AppSettings;

public sealed class NotionLikeSettings
{
    public const string SectionName = "Notion-like";

    public string Default_user_role { get; init; } = null!;
}

