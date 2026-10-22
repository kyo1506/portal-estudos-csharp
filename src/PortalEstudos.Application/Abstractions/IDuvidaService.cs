using PortalEstudos.Application.Dtos;

namespace PortalEstudos.Application.Abstractions;

public interface IDuvidaService
{
    Task<DuvidaMentoriaDto> EnviarDuvidaAsync(int usuarioId, CriarDuvidaInput input, CancellationToken ct = default);
    Task<IReadOnlyList<DuvidaMentoriaDto>> ObterMinhasDuvidasAsync(int usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<DuvidaMentoriaDto>> ObterFilaAtendimentoAsync(string? status = null, CancellationToken ct = default);
    Task<DuvidaMentoriaDto?> ObterDuvidaAsync(int duvidaId, CancellationToken ct = default);
    Task<bool> ResponderDuvidaAsync(int duvidaId, string resposta, CancellationToken ct = default);
}
