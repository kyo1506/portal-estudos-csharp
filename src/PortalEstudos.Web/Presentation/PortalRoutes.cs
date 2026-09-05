namespace PortalEstudos.Web.Presentation;

/// <summary>Centraliza a geração de rotas internas para evitar URLs espalhadas e erradas.</summary>
public static class PortalRoutes
{
    public static string Fases => "/fases";

    public static string Fase(int faseId) => $"/fase/{faseId}";

    public static string Lesson(int faseId, int lessonId) => $"/fase/{faseId}/lesson/{lessonId}";

    public static string Exercise(int faseId, int exerciseId) => $"/fase/{faseId}/exercise/{exerciseId}";
}
