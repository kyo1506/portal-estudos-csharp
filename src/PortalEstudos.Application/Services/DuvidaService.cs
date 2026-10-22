using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public sealed class DuvidaService : IDuvidaService
{
    private readonly IDuvidaRepository _duvidaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICatalogService _catalogService;

    public DuvidaService(
        IDuvidaRepository duvidaRepository,
        IUserRepository userRepository,
        ICatalogService catalogService)
    {
        _duvidaRepository = duvidaRepository;
        _userRepository = userRepository;
        _catalogService = catalogService;
    }

    public async Task<DuvidaMentoriaDto> EnviarDuvidaAsync(int usuarioId, CriarDuvidaInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Titulo))
            throw new ArgumentException("O título da dúvida é obrigatório.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.Descricao))
            throw new ArgumentException("A descrição da dúvida é obrigatória.", nameof(input));

        var duvida = new DuvidaMentoria
        {
            UsuarioId = usuarioId,
            FaseId = input.FaseId,
            LicaoId = input.LicaoId,
            ExercicioId = input.ExercicioId,
            Titulo = input.Titulo.Trim(),
            Descricao = input.Descricao.Trim(),
            TrechoCodigo = string.IsNullOrWhiteSpace(input.TrechoCodigo) ? null : input.TrechoCodigo,
            Status = DuvidaStatus.Pendente,
            CriadaEm = DateTime.UtcNow
        };

        var criada = await _duvidaRepository.CriarAsync(duvida, ct);
        var usuario = await _userRepository.GetByIdAsync(usuarioId, ct);

        return MapearParaDto(criada, usuario);
    }

    public async Task<IReadOnlyList<DuvidaMentoriaDto>> ObterMinhasDuvidasAsync(int usuarioId, CancellationToken ct = default)
    {
        var duvidas = await _duvidaRepository.ListarPorUsuarioAsync(usuarioId, ct);
        var usuario = await _userRepository.GetByIdAsync(usuarioId, ct);

        return duvidas.Select(d => MapearParaDto(d, usuario)).ToList();
    }

    public async Task<IReadOnlyList<DuvidaMentoriaDto>> ObterFilaAtendimentoAsync(string? status = null, CancellationToken ct = default)
    {
        var duvidas = await _duvidaRepository.ListarTodasAsync(status, ct);
        var resultado = new List<DuvidaMentoriaDto>();

        foreach (var d in duvidas)
        {
            var usuario = d.Usuario ?? await _userRepository.GetByIdAsync(d.UsuarioId, ct);
            resultado.Add(MapearParaDto(d, usuario));
        }

        return resultado;
    }

    public async Task<DuvidaMentoriaDto?> ObterDuvidaAsync(int duvidaId, CancellationToken ct = default)
    {
        var duvida = await _duvidaRepository.ObterPorIdAsync(duvidaId, ct);
        if (duvida == null) return null;

        var usuario = duvida.Usuario ?? await _userRepository.GetByIdAsync(duvida.UsuarioId, ct);
        return MapearParaDto(duvida, usuario);
    }

    public async Task<bool> ResponderDuvidaAsync(int duvidaId, string resposta, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(resposta))
            throw new ArgumentException("A resposta não pode ser vazia.", nameof(resposta));

        var duvida = await _duvidaRepository.ObterPorIdAsync(duvidaId, ct);
        if (duvida == null) return false;

        duvida.RespostaInstrutor = resposta.Trim();
        duvida.Status = DuvidaStatus.Respondida;
        duvida.RespondidaEm = DateTime.UtcNow;

        await _duvidaRepository.AtualizarAsync(duvida, ct);
        return true;
    }

    private DuvidaMentoriaDto MapearParaDto(DuvidaMentoria d, Usuario? u)
    {
        var fases = _catalogService.GetAllFases();
        var fase = fases.FirstOrDefault(f => f.Id == d.FaseId);
        if (fase == null && d.ExercicioId.HasValue)
        {
            fase = fases.FirstOrDefault(f => f.Exercises.Any(e => e.Id == d.ExercicioId.Value));
        }
        if (fase == null && d.LicaoId.HasValue)
        {
            fase = fases.FirstOrDefault(f => f.Lessons.Any(l => l.Id == d.LicaoId.Value));
        }

        var tituloFase = fase?.Title ?? $"Fase {d.FaseId}";

        string? tituloLicao = null;
        if (d.LicaoId.HasValue && fase != null)
        {
            tituloLicao = fase.Lessons.FirstOrDefault(l => l.Id == d.LicaoId.Value)?.Title;
        }

        string? tituloExercicio = null;
        if (d.ExercicioId.HasValue && fase != null)
        {
            tituloExercicio = fase.Exercises.FirstOrDefault(e => e.Id == d.ExercicioId.Value)?.Title;
        }

        return new DuvidaMentoriaDto(
            d.Id,
            d.UsuarioId,
            u?.Nome ?? $"Aluno #{d.UsuarioId}",
            u?.Email ?? "",
            d.FaseId,
            tituloFase,
            d.LicaoId,
            tituloLicao,
            d.ExercicioId,
            tituloExercicio,
            d.Titulo,
            d.Descricao,
            d.TrechoCodigo,
            d.RespostaInstrutor,
            d.Status,
            d.CriadaEm,
            d.RespondidaEm
        );
    }
}
