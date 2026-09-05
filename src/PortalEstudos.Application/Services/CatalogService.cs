using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface ICatalogService
{
    IReadOnlyList<Fase> GetAllFases();
    Fase? GetFase(int id);
    LessonModel? GetLesson(int faseId, int lessonId);
    ExerciseModel? GetExercise(int faseId, int exerciseId);
}

/// <summary>Consultas de leitura sobre o catálogo do curso.</summary>
public sealed class CatalogService : ICatalogService
{
    private readonly IContentRepository _content;

    public CatalogService(IContentRepository content)
    {
        _content = content;
    }

    public IReadOnlyList<Fase> GetAllFases() => _content.GetAllFases();

    public Fase? GetFase(int id) => _content.GetFase(id);

    public LessonModel? GetLesson(int faseId, int lessonId) => _content.GetLesson(faseId, lessonId);

    public ExerciseModel? GetExercise(int faseId, int exerciseId) => _content.GetExercise(faseId, exerciseId);
}
