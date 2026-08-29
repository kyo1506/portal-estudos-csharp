namespace PortalEstudos.Application.Dtos;

public class ChallengePrStatus
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // "open" | "merged" | "closed"
    public string Url { get; set; } = string.Empty;
    public bool IsMerged { get; set; }
}
