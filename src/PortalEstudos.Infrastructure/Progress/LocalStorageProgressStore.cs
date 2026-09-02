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
            // JSRuntime não está disponível durante a pré-renderização (SSR).
            // Devolvemos progresso vazio nesse momento; o componente será reexecutado
            // após a hidratação interativa e fará a leitura de verdade.
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var progress = JsonSerializer.Deserialize<UserProgress>(json);
                if (progress != null) return progress;
            }
        }
        catch (InvalidOperationException)
        {
            // Pré-renderização: JSRuntime indisponível.
        }
        catch (JSDisconnectedException)
        {
            // Circuito caiu entre o await e a continuação — irrelevante para carregar estado.
        }
        catch (JSException)
        {
            // Erro no JS (localStorage bloqueado, etc.) — devolvemos vazio.
        }
        catch
        {
            // Falha genérica de desserialização ou runtime: devolvemos progresso vazio.
        }

        return new UserProgress();
    }

    public async Task SaveAsync(UserProgress progress)
    {
        var json = JsonSerializer.Serialize(progress);
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (InvalidOperationException) { /* SSR */ }
        catch (JSDisconnectedException) { /* circuito caiu */ }
        catch (JSException) { /* erro JS */ }
    }

    public async Task ResetAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
        catch (InvalidOperationException) { /* SSR */ }
        catch (JSDisconnectedException) { /* circuito caiu */ }
        catch (JSException) { /* erro JS */ }
    }
}