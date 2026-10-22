using Microsoft.EntityFrameworkCore;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Data;

public sealed class EfUserProgressRepository : IUserProgressRepository
{
    private readonly PortalEstudosDbContext _context;

    public EfUserProgressRepository(PortalEstudosDbContext context)
    {
        _context = context;
    }

    public async Task<UserProgress> GetProgressAsync(int usuarioId, CancellationToken ct = default)
    {
        var progress = new UserProgress();

        var licoes = await _context.ProgressoLicoes
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync(ct);

        foreach (var licao in licoes)
        {
            progress.CompletedLessons.Add(licao.LicaoId);
            if (licao.ConcluidoEm > progress.LastActivity)
            {
                progress.LastActivity = licao.ConcluidoEm;
            }
        }

        var submissoes = await _context.SubmissoesExercicios
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .ToListAsync(ct);

        foreach (var sub in submissoes)
        {
            if (sub.PassouNosTestes)
            {
                progress.CompletedExercises.Add(sub.ExercicioId);
            }

            if (!string.IsNullOrEmpty(sub.CodigoSubmetido))
            {
                progress.ExerciseAnswers[sub.ExercicioId] = sub.CodigoSubmetido;
            }

            if (sub.AtualizadoEm > progress.LastActivity)
            {
                progress.LastActivity = sub.AtualizadoEm;
            }
        }

        return progress;
    }

    public async Task MarkLessonAsync(int usuarioId, int faseId, int lessonId, bool completed, CancellationToken ct = default)
    {
        var existing = await _context.ProgressoLicoes
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.LicaoId == lessonId, ct);

        if (completed)
        {
            if (existing == null)
            {
                _context.ProgressoLicoes.Add(new ProgressoLicao
                {
                    UsuarioId = usuarioId,
                    FaseId = faseId,
                    LicaoId = lessonId,
                    ConcluidoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(ct);
            }
        }
        else
        {
            if (existing != null)
            {
                _context.ProgressoLicoes.Remove(existing);
                await _context.SaveChangesAsync(ct);
            }
        }
    }

    public async Task RecordExerciseSubmissionAsync(int usuarioId, int faseId, int exerciseId, string code, bool isCorrect, string? errorMessage = null, CancellationToken ct = default)
    {
        var existing = await _context.SubmissoesExercicios
            .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.ExercicioId == exerciseId, ct);

        if (existing != null)
        {
            existing.Tentativas++;
            existing.CodigoSubmetido = code;
            existing.CompilouComSucesso = true;
            existing.PassouNosTestes = isCorrect || existing.PassouNosTestes;
            existing.MensagemErro = errorMessage;
            existing.AtualizadoEm = DateTime.UtcNow;
            if (faseId > 0) existing.FaseId = faseId;
        }
        else
        {
            _context.SubmissoesExercicios.Add(new SubmissaoExercicio
            {
                UsuarioId = usuarioId,
                FaseId = faseId,
                ExercicioId = exerciseId,
                CodigoSubmetido = code,
                CompilouComSucesso = true,
                PassouNosTestes = isCorrect,
                MensagemErro = errorMessage,
                Tentativas = 1,
                AtualizadoEm = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task MergeProgressAsync(int usuarioId, UserProgress localProgress, CancellationToken ct = default)
    {
        if (localProgress.CompletedLessons.Count == 0 && localProgress.CompletedExercises.Count == 0)
            return;

        var existingLessons = await _context.ProgressoLicoes
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.LicaoId)
            .ToHashSetAsync(ct);

        foreach (var lessonId in localProgress.CompletedLessons)
        {
            if (!existingLessons.Contains(lessonId))
            {
                _context.ProgressoLicoes.Add(new ProgressoLicao
                {
                    UsuarioId = usuarioId,
                    FaseId = 0,
                    LicaoId = lessonId,
                    ConcluidoEm = DateTime.UtcNow
                });
            }
        }

        var existingSubmissions = await _context.SubmissoesExercicios
            .Where(s => s.UsuarioId == usuarioId)
            .ToDictionaryAsync(s => s.ExercicioId, ct);

        foreach (var exerciseId in localProgress.CompletedExercises)
        {
            localProgress.ExerciseAnswers.TryGetValue(exerciseId, out var code);
            code ??= string.Empty;

            if (existingSubmissions.TryGetValue(exerciseId, out var existing))
            {
                existing.PassouNosTestes = true;
                if (!string.IsNullOrEmpty(code) && string.IsNullOrEmpty(existing.CodigoSubmetido))
                {
                    existing.CodigoSubmetido = code;
                }
            }
            else
            {
                _context.SubmissoesExercicios.Add(new SubmissaoExercicio
                {
                    UsuarioId = usuarioId,
                    FaseId = 0,
                    ExercicioId = exerciseId,
                    CodigoSubmetido = code,
                    CompilouComSucesso = true,
                    PassouNosTestes = true,
                    Tentativas = 1,
                    AtualizadoEm = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SubmissaoExercicio>> GetSubmissionsByUserAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _context.SubmissoesExercicios
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.AtualizadoEm)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SubmissaoExercicio>> GetAllSubmissionsAsync(CancellationToken ct = default)
    {
        return await _context.SubmissoesExercicios
            .AsNoTracking()
            .Include(s => s.Usuario)
            .OrderByDescending(s => s.AtualizadoEm)
            .ToListAsync(ct);
    }
}
