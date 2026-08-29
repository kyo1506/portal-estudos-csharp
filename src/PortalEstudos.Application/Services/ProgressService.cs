using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface IProgressService
{
    Task<UserProgress> LoadAsync();
    Task SaveAsync(UserProgress progress);
    Task MarkLessonCompleteAsync(int lessonId);
    Task MarkLessonIncompleteAsync(int lessonId);
    Task MarkExerciseCompleteAsync(int exerciseId, string? answer = null);
    Task MarkChallengeCompleteAsync(int challengeId);
    Task ResetAsync();
}

/// <summary>Operações de domínio sobre o progresso do aluno, com cache em memória
/// (escopo por circuito) para evitar leituras repetidas do localStorage a cada ação.</summary>
public sealed class ProgressService : IProgressService
{
    private readonly IProgressStore _store;
    private UserProgress? _cache;

    public ProgressService(IProgressStore store)
    {
        _store = store;
    }

    public async Task<UserProgress> LoadAsync()
    {
        _cache ??= await _store.LoadAsync();
        return _cache;
    }

    public async Task SaveAsync(UserProgress progress)
    {
        _cache = progress;
        await _store.SaveAsync(progress);
    }

    public async Task MarkLessonCompleteAsync(int lessonId)
    {
        var p = await LoadAsync();
        if (p.CompletedLessons.Add(lessonId))
        {
            p.LastActivity = DateTime.Now;
            await SaveAsync(p);
        }
    }

    public async Task MarkLessonIncompleteAsync(int lessonId)
    {
        var p = await LoadAsync();
        if (p.CompletedLessons.Remove(lessonId))
        {
            await SaveAsync(p);
        }
    }

    public async Task MarkExerciseCompleteAsync(int exerciseId, string? answer = null)
    {
        var p = await LoadAsync();
        p.CompletedExercises.Add(exerciseId);
        if (!string.IsNullOrEmpty(answer))
            p.ExerciseAnswers[exerciseId] = answer;
        p.LastActivity = DateTime.Now;
        await SaveAsync(p);
    }

    public async Task MarkChallengeCompleteAsync(int challengeId)
    {
        var p = await LoadAsync();
        if (p.CompletedChallenges.Add(challengeId))
        {
            await SaveAsync(p);
        }
    }

    public async Task ResetAsync()
    {
        _cache = null;
        await _store.ResetAsync();
    }
}
