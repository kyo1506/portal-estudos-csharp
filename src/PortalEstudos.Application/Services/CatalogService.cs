using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface ICatalogService
{
    IReadOnlyList<Week> GetAllWeeks();
    Week? GetWeek(int id);
    LessonModel? GetLesson(int weekId, int lessonId);
    ExerciseModel? GetExercise(int weekId, int exerciseId);
}

/// <summary>Consultas de leitura sobre o catálogo do curso.</summary>
public sealed class CatalogService : ICatalogService
{
    private readonly IContentRepository _content;

    public CatalogService(IContentRepository content)
    {
        _content = content;
    }

    public IReadOnlyList<Week> GetAllWeeks() => _content.GetAllWeeks();

    public Week? GetWeek(int id) => _content.GetWeek(id);

    public LessonModel? GetLesson(int weekId, int lessonId) => _content.GetLesson(weekId, lessonId);

    public ExerciseModel? GetExercise(int weekId, int exerciseId) => _content.GetExercise(weekId, exerciseId);
}
