using MudBlazor;
using PortalEstudos.Domain.Enums;

namespace PortalEstudos.Web.Presentation;

/// <summary>Mapeia a dificuldade de um exercício para cor e rótulo da UI (pt-BR).</summary>
public static class DifficultyPresentation
{
    public static Color GetColor(ExerciseDifficulty difficulty) => difficulty switch
    {
        ExerciseDifficulty.Easy => Color.Success,
        ExerciseDifficulty.Medium => Color.Warning,
        ExerciseDifficulty.Hard => Color.Error,
        _ => Color.Default
    };

    public static string GetText(ExerciseDifficulty difficulty) => difficulty switch
    {
        ExerciseDifficulty.Easy => "Fácil",
        ExerciseDifficulty.Medium => "Médio",
        ExerciseDifficulty.Hard => "Difícil",
        _ => "Desconhecido"
    };
}
