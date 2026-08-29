using System.Text.Json;
using Microsoft.JSInterop;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Progress;

/// <summary>Persiste o progresso do aluno no <c>localStorage</c> do navegador.</summary>
public sealed class LocalStorageProgressStore : IProgressStore
{
    private const string StorageKey = "portal-estudos-progress";

    private readonly IJSRuntime _js;

    public LocalStorageProgressStore(IJSRuntime js)
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
            // Ignora erros de desserialização e retorna progresso vazio.
        }

        return new UserProgress();
    }

    public async Task SaveAsync(UserProgress progress)
    {
        var json = JsonSerializer.Serialize(progress);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task ResetAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}
