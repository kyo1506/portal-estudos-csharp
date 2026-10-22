namespace PortalEstudos.Application.Dtos;

public record DuvidaMentoriaDto(
    int Id,
    int UsuarioId,
    string NomeAluno,
    string EmailAluno,
    int FaseId,
    string TituloFase,
    int? LicaoId,
    string? TituloLicao,
    int? ExercicioId,
    string? TituloExercicio,
    string Titulo,
    string Descricao,
    string? TrechoCodigo,
    string? RespostaInstrutor,
    string Status,
    DateTime CriadaEm,
    DateTime? RespondidaEm
);

public record CriarDuvidaInput(
    int FaseId,
    int? LicaoId,
    int? ExercicioId,
    string Titulo,
    string Descricao,
    string? TrechoCodigo
);
