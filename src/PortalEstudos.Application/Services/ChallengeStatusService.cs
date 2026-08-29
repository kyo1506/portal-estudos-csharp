using Microsoft.Extensions.Caching.Memory;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;

namespace PortalEstudos.Application.Services;

public interface IChallengeStatusService
{
    Task<IReadOnlyList<ChallengePrStatus>> GetAsync();
}

/// <summary>Consulta o status de PRs de desafios com cache em memória para evitar
/// chamadas repetidas e exaustão da cota da API do GitHub.</summary>
public sealed class ChallengeStatusService : IChallengeStatusService
{
    private const string CacheKey = "challenge-pr-status";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IGitHubApi _gitHub;
    private readonly IMemoryCache _cache;

    public ChallengeStatusService(IGitHubApi gitHub, IMemoryCache cache)
    {
        _gitHub = gitHub;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ChallengePrStatus>> GetAsync()
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<ChallengePrStatus>? cached) && cached != null)
        {
            return cached;
        }

        var result = await _gitHub.GetChallengePrStatusAsync();

        _cache.Set(CacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheTtl
        });

        return result;
    }
}
