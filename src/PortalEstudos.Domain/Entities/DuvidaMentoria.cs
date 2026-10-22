namespace PortalEstudos.Domain.Entities;

/// <summary>Registra uma dúvida pedagógica contextual enviada pelo aluno para a mentoria.</summary>
public sealed class DuvidaMentoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int FaseId { get; set; }
    public int? LicaoId { get; set; }
    public int? ExercicioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? TrechoCodigo { get; set; }
    public string? RespostaInstrutor { get; set; }
    public string Status { get; set; } = DuvidaStatus.Pendente;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public DateTime? RespondidaEm { get; set; }

    public Usuario? Usuario { get; set; }
}

public static class DuvidaStatus
{
    public const string Pendente = "Pendente";
    public const string Respondida = "Respondida";
}
