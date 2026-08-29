using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

/// <summary>Persistência do progresso do aluno no navegador.</summary>
public interface IProgressStore
{
    Task<UserProgress> LoadAsync();
    Task SaveAsync(UserProgress progress);
    Task ResetAsync();
}
