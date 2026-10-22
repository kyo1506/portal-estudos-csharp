using Microsoft.EntityFrameworkCore;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Data;

public class PortalEstudosDbContext : DbContext
{
    public PortalEstudosDbContext(DbContextOptions<PortalEstudosDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ProgressoLicao> ProgressoLicoes => Set<ProgressoLicao>();
    public DbSet<SubmissaoExercicio> SubmissoesExercicios => Set<SubmissaoExercicio>();
    public DbSet<DuvidaMentoria> DuvidasMentoria => Set<DuvidaMentoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.SenhaHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.CriadoEm).IsRequired();
        });

        modelBuilder.Entity<ProgressoLicao>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => new { p.UsuarioId, p.FaseId, p.LicaoId }).IsUnique();
            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.ProgressoLicoes)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubmissaoExercicio>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.UsuarioId, s.FaseId, s.ExercicioId });
            entity.HasOne(s => s.Usuario)
                .WithMany(u => u.SubmissoesExercicios)
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DuvidaMentoria>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Descricao).IsRequired();
            entity.Property(d => d.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(d => new { d.UsuarioId, d.Status });
            entity.HasOne(d => d.Usuario)
                .WithMany(u => u.DuvidasMentoria)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
