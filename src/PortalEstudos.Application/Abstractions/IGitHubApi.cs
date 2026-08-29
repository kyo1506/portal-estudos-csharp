using PortalEstudos.Application.Dtos;

namespace PortalEstudos.Application.Abstractions;

/// <summary>Consulta o status de Pull Requests de desafios no GitHub.</summary>
public interface IGitHubApi
{
    Task<IReadOnlyList<ChallengePrStatus>> GetChallengePrStatusAsync();
}
