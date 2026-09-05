using System.Reflection;
using System.Text.Json;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Content;

/// <summary>Lê o conteúdo do curso de um arquivo JSON embutido como recurso
/// (<c>Content/ContentSeed.json</c>), separando dados de comportamento.</summary>
public sealed class JsonContentRepository : IContentRepository
{
    private readonly List<Fase> _fases;

    public JsonContentRepository()
    {
        _fases = LoadSeed();
    }

    public IReadOnlyList<Fase> GetAllFases() => _fases;

    public Fase? GetFase(int id) => _fases.FirstOrDefault(w => w.Id == id);

    public LessonModel? GetLesson(int faseId, int lessonId)
        => GetFase(faseId)?.Lessons.FirstOrDefault(l => l.Id == lessonId);

    public ExerciseModel? GetExercise(int faseId, int exerciseId)
        => GetFase(faseId)?.Exercises.FirstOrDefault(e => e.Id == exerciseId);

    private static List<Fase> LoadSeed()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("ContentSeed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Recurso embutido 'ContentSeed.json' não encontrado.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Não foi possível abrir o recurso '{resourceName}'.");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var file = JsonSerializer.Deserialize<ContentFile>(stream, options)
            ?? throw new InvalidOperationException("Falha ao desserializar o conteúdo do curso.");

        return file.Fases;
    }

    private sealed class ContentFile
    {
        public List<Fase> Fases { get; init; } = new();
    }
}
