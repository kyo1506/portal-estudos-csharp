using PortalEstudos.Application.Dtos;

namespace PortalEstudos.Application.Abstractions;

public interface IMentoriaService
{
    Task<TurmaOverviewDto> GetTurmaOverviewAsync(CancellationToken ct = default);
    Task<AlunoDetalhesDto?> GetAlunoDetalhesAsync(int alunoId, CancellationToken ct = default);
}
