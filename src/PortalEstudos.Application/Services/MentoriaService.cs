using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public sealed class MentoriaService : IMentoriaService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProgressRepository _progressRepository;
    private readonly ICatalogService _catalogService;

    public MentoriaService(
        IUserRepository userRepository,
        IUserProgressRepository progressRepository,
        ICatalogService catalogService)
    {
        _userRepository = userRepository;
        _progressRepository = progressRepository;
        _catalogService = catalogService;
    }

    public async Task<TurmaOverviewDto> GetTurmaOverviewAsync(CancellationToken ct = default)
    {
        var allUsers = await _userRepository.GetAllAsync(ct);
        var alunos = allUsers.Where(u => u.Role == Roles.Aluno).ToList();

        var fases = _catalogService.GetAllFases();
        var totalItensTrilha = fases.Sum(f => f.Lessons.Count + f.Exercises.Count);
        if (totalItensTrilha == 0) totalItensTrilha = 1;

        var alunosResumo = new List<AlunoProgressoResumoDto>();
        var totalLicoesAcumuladas = 0;
        var totalExerciciosAcumulados = 0;

        foreach (var aluno in alunos)
        {
            var progress = await _progressRepository.GetProgressAsync(aluno.Id, ct);
            var licoes = progress.CompletedLessons.Count;
            var exercicios = progress.CompletedExercises.Count;

            totalLicoesAcumuladas += licoes;
            totalExerciciosAcumulados += exercicios;

            var percentual = Math.Round(((double)(licoes + exercicios) / totalItensTrilha) * 100, 1);
            if (percentual > 100) percentual = 100;

            DateTime? ultimaAtividade = progress.LastActivity > DateTime.MinValue ? progress.LastActivity : null;

            alunosResumo.Add(new AlunoProgressoResumoDto(
                aluno.Id,
                aluno.Nome,
                aluno.Email,
                licoes,
                exercicios,
                percentual,
                ultimaAtividade
            ));
        }

        var allSubmissions = await _progressRepository.GetAllSubmissionsAsync(ct);

        var alertas = new List<AlertaDificuldadeDto>();
        foreach (var sub in allSubmissions.Where(s => s.Tentativas >= 3))
        {
            var aluno = alunos.FirstOrDefault(a => a.Id == sub.UsuarioId);
            var nomeAluno = aluno?.Nome ?? sub.Usuario?.Nome ?? $"Aluno #{sub.UsuarioId}";

            var (tituloFase, tituloExercicio) = ObterTitulos(sub.FaseId, sub.ExercicioId);

            alertas.Add(new AlertaDificuldadeDto(
                sub.UsuarioId,
                nomeAluno,
                sub.FaseId,
                tituloFase,
                sub.ExercicioId,
                tituloExercicio,
                sub.Tentativas,
                sub.MensagemErro,
                sub.CodigoSubmetido,
                sub.AtualizadoEm
            ));
        }

        var totalAlunos = alunos.Count;
        var mediaLicoes = totalAlunos > 0 ? Math.Round((double)totalLicoesAcumuladas / totalAlunos, 1) : 0;
        var mediaExercicios = totalAlunos > 0 ? Math.Round((double)totalExerciciosAcumulados / totalAlunos, 1) : 0;

        return new TurmaOverviewDto(
            totalAlunos,
            mediaLicoes,
            mediaExercicios,
            alertas.Count,
            alunosResumo.OrderByDescending(a => a.PercentualGeral).ThenBy(a => a.Nome).ToList(),
            alertas.OrderByDescending(a => a.Tentativas).ThenByDescending(a => a.AtualizadoEm).ToList()
        );
    }

    public async Task<AlunoDetalhesDto?> GetAlunoDetalhesAsync(int alunoId, CancellationToken ct = default)
    {
        var aluno = await _userRepository.GetByIdAsync(alunoId, ct);
        if (aluno == null)
            return null;

        var progress = await _progressRepository.GetProgressAsync(alunoId, ct);
        var submissoesDb = await _progressRepository.GetSubmissionsByUserAsync(alunoId, ct);

        var submissoesDetalhes = new List<SubmissaoDetalheDto>();
        foreach (var sub in submissoesDb)
        {
            var (tituloFase, tituloExercicio) = ObterTitulos(sub.FaseId, sub.ExercicioId);

            submissoesDetalhes.Add(new SubmissaoDetalheDto(
                sub.ExercicioId,
                sub.FaseId,
                tituloFase,
                tituloExercicio,
                sub.CodigoSubmetido,
                sub.PassouNosTestes,
                sub.Tentativas,
                sub.MensagemErro,
                sub.AtualizadoEm
            ));
        }

        return new AlunoDetalhesDto(
            aluno.Id,
            aluno.Nome,
            aluno.Email,
            aluno.CriadoEm,
            progress.CompletedLessons.Count,
            progress.CompletedExercises.Count,
            submissoesDetalhes.OrderByDescending(s => s.AtualizadoEm).ToList()
        );
    }

    private (string TituloFase, string TituloExercicio) ObterTitulos(int faseId, int exercicioId)
    {
        var fases = _catalogService.GetAllFases();
        var fase = fases.FirstOrDefault(f => f.Id == faseId);

        if (fase == null)
        {
            fase = fases.FirstOrDefault(f => f.Exercises.Any(e => e.Id == exercicioId));
        }

        var tituloFase = fase?.Title ?? $"Fase {faseId}";
        var exercicio = fase?.Exercises.FirstOrDefault(e => e.Id == exercicioId);
        var tituloExercicio = exercicio?.Title ?? $"Exercício #{exercicioId}";

        return (tituloFase, tituloExercicio);
    }
}
