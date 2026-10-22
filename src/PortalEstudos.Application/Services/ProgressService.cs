using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface IProgressService
{
    event Action? OnChange;
    Task<UserProgress> LoadAsync(bool forceReload = false);
    Task SaveAsync(UserProgress progress);
    Task MarkLessonCompleteAsync(int lessonId, int? faseId = null);
    Task MarkLessonIncompleteAsync(int lessonId, int? faseId = null);
    Task MarkExerciseCompleteAsync(int exerciseId, string? answer = null, int? faseId = null);
    Task RecordExerciseAttemptAsync(int exerciseId, string code, bool isCorrect, string? errorMessage = null, int? faseId = null);
    Task MarkChallengeCompleteAsync(int challengeId);
    Task ResetAsync();
}

/// <summary>Operações de domínio sobre o progresso do aluno com suporte híbrido:
/// persistência em nuvem (EF Core) para usuários autenticados e localStorage para uso anônimo/offline.</summary>
public sealed class ProgressService : IProgressService
{
    private readonly IProgressStore _store;
    private readonly IUserProgressRepository? _userProgressRepository;
    private readonly ICurrentUserAccessor? _currentUserAccessor;
    private readonly ICatalogService? _catalogService;

    private UserProgress? _cache;

    public event Action? OnChange;

    public ProgressService(
        IProgressStore store,
        IUserProgressRepository? userProgressRepository = null,
        ICurrentUserAccessor? currentUserAccessor = null,
        ICatalogService? catalogService = null)
    {
        _store = store;
        _userProgressRepository = userProgressRepository;
        _currentUserAccessor = currentUserAccessor;
        _catalogService = catalogService;
    }

    public async Task<UserProgress> LoadAsync(bool forceReload = false)
    {
        if (_cache != null && !forceReload)
        {
            return _cache;
        }

        int? userId = null;
        if (_currentUserAccessor != null)
        {
            userId = await _currentUserAccessor.GetCurrentUserIdAsync();
        }

        if (userId.HasValue && _userProgressRepository != null)
        {
            var dbProgress = await _userProgressRepository.GetProgressAsync(userId.Value);
            var localProgress = await _store.LoadAsync();

            var hasPendingLocalLessons = localProgress.CompletedLessons.Any(l => !dbProgress.CompletedLessons.Contains(l));
            var hasPendingLocalExercises = localProgress.CompletedExercises.Any(e => !dbProgress.CompletedExercises.Contains(e));

            if (hasPendingLocalLessons || hasPendingLocalExercises)
            {
                await _userProgressRepository.MergeProgressAsync(userId.Value, localProgress);
                dbProgress = await _userProgressRepository.GetProgressAsync(userId.Value);
            }

            _cache = dbProgress;
            await _store.SaveAsync(dbProgress);
            return _cache;
        }

        _cache = await _store.LoadAsync();
        return _cache;
    }

    public async Task SaveAsync(UserProgress progress)
    {
        _cache = progress;
        await _store.SaveAsync(progress);
        OnChange?.Invoke();
    }

    public async Task MarkLessonCompleteAsync(int lessonId, int? faseId = null)
    {
        var p = await LoadAsync();
        if (p.CompletedLessons.Add(lessonId))
        {
            p.LastActivity = DateTime.Now;

            int? userId = null;
            if (_currentUserAccessor != null)
            {
                userId = await _currentUserAccessor.GetCurrentUserIdAsync();
            }

            if (userId.HasValue && _userProgressRepository != null)
            {
                var resolvedFaseId = faseId ?? ResolveFaseIdForLesson(lessonId);
                await _userProgressRepository.MarkLessonAsync(userId.Value, resolvedFaseId, lessonId, completed: true);
            }

            await SaveAsync(p);
        }
    }

    public async Task MarkLessonIncompleteAsync(int lessonId, int? faseId = null)
    {
        var p = await LoadAsync();
        if (p.CompletedLessons.Remove(lessonId))
        {
            int? userId = null;
            if (_currentUserAccessor != null)
            {
                userId = await _currentUserAccessor.GetCurrentUserIdAsync();
            }

            if (userId.HasValue && _userProgressRepository != null)
            {
                var resolvedFaseId = faseId ?? ResolveFaseIdForLesson(lessonId);
                await _userProgressRepository.MarkLessonAsync(userId.Value, resolvedFaseId, lessonId, completed: false);
            }

            await SaveAsync(p);
        }
    }

    public async Task MarkExerciseCompleteAsync(int exerciseId, string? answer = null, int? faseId = null)
    {
        await RecordExerciseAttemptAsync(exerciseId, answer ?? string.Empty, isCorrect: true, errorMessage: null, faseId: faseId);
    }

    public async Task RecordExerciseAttemptAsync(int exerciseId, string code, bool isCorrect, string? errorMessage = null, int? faseId = null)
    {
        var p = await LoadAsync();

        if (isCorrect)
        {
            p.CompletedExercises.Add(exerciseId);
        }

        if (!string.IsNullOrEmpty(code))
        {
            p.ExerciseAnswers[exerciseId] = code;
        }

        p.LastActivity = DateTime.Now;

        int? userId = null;
        if (_currentUserAccessor != null)
        {
            userId = await _currentUserAccessor.GetCurrentUserIdAsync();
        }

        if (userId.HasValue && _userProgressRepository != null)
        {
            var resolvedFaseId = faseId ?? ResolveFaseIdForExercise(exerciseId);
            await _userProgressRepository.RecordExerciseSubmissionAsync(
                userId.Value,
                resolvedFaseId,
                exerciseId,
                code,
                isCorrect: isCorrect,
                errorMessage: errorMessage);
        }

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
        OnChange?.Invoke();
    }

    private int ResolveFaseIdForLesson(int lessonId)
    {
        if (_catalogService == null) return 0;
        foreach (var fase in _catalogService.GetAllFases())
        {
            if (fase.Lessons.Any(l => l.Id == lessonId))
                return fase.Id;
        }
        return 0;
    }

    private int ResolveFaseIdForExercise(int exerciseId)
    {
        if (_catalogService == null) return 0;
        foreach (var fase in _catalogService.GetAllFases())
        {
            if (fase.Exercises.Any(e => e.Id == exerciseId))
                return fase.Id;
        }
        return 0;
    }
}
