namespace PortalEstudos.Domain.Entities;

/// <summary>Uma semana de estudo do curso.</summary>
public class Week
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = "📚";
    public List<LessonModel> Lessons { get; init; } = new();
    public List<ExerciseModel> Exercises { get; init; } = new();
    public Challenge? Challenge { get; init; }
}
