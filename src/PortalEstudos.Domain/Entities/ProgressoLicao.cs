namespace PortalEstudos.Domain.Entities;

/// <summary>Registra a conclusão de uma lição teórica por um usuário.</summary>
public sealed class ProgressoLicao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int FaseId { get; set; }
    public int LicaoId { get; set; }
    public DateTime ConcluidoEm { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
