using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

public interface IUserProgressRepository
{
    Task<UserProgress> GetProgressAsync(int usuarioId, CancellationToken ct = default);
    Task MarkLessonAsync(int usuarioId, int faseId, int lessonId, bool completed, CancellationToken ct = default);
    Task RecordExerciseSubmissionAsync(int usuarioId, int faseId, int exerciseId, string code, bool isCorrect, string? errorMessage = null, CancellationToken ct = default);
    Task MergeProgressAsync(int usuarioId, UserProgress localProgress, CancellationToken ct = default);
    Task<IReadOnlyList<SubmissaoExercicio>> GetSubmissionsByUserAsync(int usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<SubmissaoExercicio>> GetAllSubmissionsAsync(CancellationToken ct = default);
}
