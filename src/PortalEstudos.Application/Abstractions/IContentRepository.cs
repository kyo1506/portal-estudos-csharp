using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

/// <summary>Fonte de dados do conteúdo do curso (adapta-se a JSON, banco etc.).</summary>
public interface IContentRepository
{
    IReadOnlyList<Week> GetAllWeeks();
    Week? GetWeek(int id);
    LessonModel? GetLesson(int weekId, int lessonId);
    ExerciseModel? GetExercise(int weekId, int exerciseId);
}
