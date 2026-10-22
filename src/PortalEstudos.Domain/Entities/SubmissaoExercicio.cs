namespace PortalEstudos.Domain.Entities;

/// <summary>Registra uma submissão de código de exercício por um usuário.</summary>
public sealed class SubmissaoExercicio
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int FaseId { get; set; }
    public int ExercicioId { get; set; }
    public string CodigoSubmetido { get; set; } = string.Empty;
    public bool CompilouComSucesso { get; set; }
    public bool PassouNosTestes { get; set; }
    public string? MensagemErro { get; set; }
    public int Tentativas { get; set; } = 1;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
