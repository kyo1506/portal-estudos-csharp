namespace PortalEstudos.Domain.Entities;

/// <summary>Uma fase (módulo) do curso.</summary>
public class Fase
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = "📚";
    public List<LessonModel> Lessons { get; init; } = new();
    public List<ExerciseModel> Exercises { get; init; } = new();
    public Challenge? Challenge { get; init; }
}
