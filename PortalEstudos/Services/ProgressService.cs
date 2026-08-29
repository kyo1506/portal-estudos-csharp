using Microsoft.JSInterop;
using System.Text.Json;
using PortalEstudos.Models;

namespace PortalEstudos.Services;

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

public class ProgressService : IProgressService
{
    private readonly IJSRuntime _js;
    private const string StorageKey = "portal-estudos-progress";

    public ProgressService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<UserProgress> LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var progress = JsonSerializer.Deserialize<UserProgress>(json);
                if (progress != null) return progress;
            }
        }
        catch
        {
            // Ignora erros de desserialização e retorna progresso vazio
        }
        return new UserProgress();
    }

    public async Task SaveAsync(UserProgress progress)
    {
        var json = JsonSerializer.Serialize(progress);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task MarkLessonCompleteAsync(int lessonId)
    {
        var p = await LoadAsync();
        p.CompletedLessons.Add(lessonId);
        p.LastActivity = DateTime.Now;
        await SaveAsync(p);
    }

    public async Task MarkLessonIncompleteAsync(int lessonId)
    {
        var p = await LoadAsync();
        p.CompletedLessons.Remove(lessonId);
        await SaveAsync(p);
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
        p.CompletedChallenges.Add(challengeId);
        await SaveAsync(p);
    }

    public async Task ResetAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}
