using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

public interface IDuvidaRepository
{
    Task<DuvidaMentoria> CriarAsync(DuvidaMentoria duvida, CancellationToken ct = default);
    Task<DuvidaMentoria?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DuvidaMentoria>> ListarPorUsuarioAsync(int usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<DuvidaMentoria>> ListarTodasAsync(string? status = null, CancellationToken ct = default);
    Task AtualizarAsync(DuvidaMentoria duvida, CancellationToken ct = default);
}
