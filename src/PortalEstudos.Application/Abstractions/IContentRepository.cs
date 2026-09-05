using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

/// <summary>Fonte de dados do conteúdo do curso (adapta-se a JSON, banco etc.).</summary>
public interface IContentRepository
{
    IReadOnlyList<Fase> GetAllFases();
    Fase? GetFase(int id);
    LessonModel? GetLesson(int faseId, int lessonId);
    ExerciseModel? GetExercise(int faseId, int exerciseId);
}
