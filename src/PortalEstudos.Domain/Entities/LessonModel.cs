namespace PortalEstudos.Domain.Entities;

public class LessonModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? CodeExample { get; init; }
    public string? CodeLanguage { get; init; }
    public int Order { get; init; }
}
