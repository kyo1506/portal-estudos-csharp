namespace PortalEstudos.Web.Presentation;

/// <summary>Centraliza a geração de rotas internas para evitar URLs espalhadas e erradas.</summary>
public static class PortalRoutes
{
    public static string Weeks => "/weeks";

    public static string Week(int weekId) => $"/week/{weekId}";

    public static string Lesson(int weekId, int lessonId) => $"/week/{weekId}/lesson/{lessonId}";

    public static string Exercise(int weekId, int exerciseId) => $"/week/{weekId}/exercise/{exerciseId}";
}
