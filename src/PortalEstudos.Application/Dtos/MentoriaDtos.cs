namespace PortalEstudos.Application.Dtos;

public record TurmaOverviewDto(
    int TotalAlunos,
    double MediaLicoesConcluidas,
    double MediaExerciciosConcluidos,
    int TotalAlertasDificuldade,
    List<AlunoProgressoResumoDto> Alunos,
    List<AlertaDificuldadeDto> Alertas
);

public record AlunoProgressoResumoDto(
    int AlunoId,
    string Nome,
    string Email,
    int TotalLicoes,
    int TotalExercicios,
    double PercentualGeral,
    DateTime? UltimaAtividade
);

public record AlunoDetalhesDto(
    int AlunoId,
    string Nome,
    string Email,
    DateTime CriadoEm,
    int TotalLicoes,
    int TotalExercicios,
    List<SubmissaoDetalheDto> Submissoes
);

public record SubmissaoDetalheDto(
    int ExercicioId,
    int FaseId,
    string TituloFase,
    string TituloExercicio,
    string CodigoSubmetido,
    bool PassouNosTestes,
    int Tentativas,
    string? MensagemErro,
    DateTime AtualizadoEm
);

public record AlertaDificuldadeDto(
    int AlunoId,
    string NomeAluno,
    int FaseId,
    string TituloFase,
    int ExercicioId,
    string TituloExercicio,
    int Tentativas,
    string? UltimoErro,
    string CodigoSubmetido,
    DateTime AtualizadoEm
);
