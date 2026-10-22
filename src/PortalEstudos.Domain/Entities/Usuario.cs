namespace PortalEstudos.Domain.Entities;

/// <summary>Representa um usuário autenticado no portal (Aluno ou Instrutor).</summary>
public sealed class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Aluno;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public List<ProgressoLicao> ProgressoLicoes { get; set; } = new();
    public List<SubmissaoExercicio> SubmissoesExercicios { get; set; } = new();
    public List<DuvidaMentoria> DuvidasMentoria { get; set; } = new();
}

public static class Roles
{
    public const string Aluno = "Aluno";
    public const string Instrutor = "Instrutor";
}
