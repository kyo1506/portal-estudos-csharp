using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PortalEstudos.Services;

public interface IGitHubService
{
    Task<List<ChallengePrStatus>> GetChallengePrStatusAsync();
}

public class ChallengePrStatus
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // "open" | "merged" | "closed"
    public string Url { get; set; } = string.Empty;
    public bool IsMerged { get; set; }
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _http;
    private const string RepoOwner = "kyo1506";
    private const string RepoName = "fundamentos-csharp";

    public GitHubService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Add("User-Agent", "PortalEstudos");
        _http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    }

    public async Task<List<ChallengePrStatus>> GetChallengePrStatusAsync()
    {
        var result = new List<ChallengePrStatus>();
        try
        {
            // Busca PRs abertos e fechados/merged (api pagina; usamos state=all)
            var prs = await _http.GetFromJsonAsync<List<GitHubPull>>(
                $"https://api.github.com/repos/{RepoOwner}/{RepoName}/pulls?state=all&per_page=100");

            if (prs == null) return result;

            foreach (var pr in prs)
            {
                var challengeId = ExtractChallengeId(pr.Title, pr.Body ?? "");
                if (challengeId == null) continue;

                result.Add(new ChallengePrStatus
                {
                    ChallengeId = challengeId.Value,
                    Title = pr.Title,
                    State = pr.State,
                    Url = pr.HtmlUrl,
                    IsMerged = pr.Merged ?? false
                });
            }
        }
        catch
        {
            // Falha silenciosa: o portal ainda mostra o desafio, só sem status de PR
        }
        return result;
    }

    private static int? ExtractChallengeId(string title, string body)
    {
        // Procura "Desafio Semana N" ou "Semana-N" no título/corpo
        var match = System.Text.RegularExpressions.Regex.Match(
            $"{title} {body}", @"[Ss]emana[-\s]?(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
            return id;
        return null;
    }
}

// DTOs mínimos da API de PRs do GitHub
public class GitHubPull
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("body")] public string? Body { get; set; }
    [JsonPropertyName("state")] public string State { get; set; } = "";
    [JsonPropertyName("merged")] public bool? Merged { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = "";
}
