using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;

namespace PortalEstudos.Infrastructure.GitHub;

/// <summary>Cliente da API de Pull Requests do GitHub para status de desafios.</summary>
public sealed class GitHubApiClient : IGitHubApi
{
    private const string RepoOwner = "kyo1506";
    private const string RepoName = "fundamentos-csharp";

    private readonly HttpClient _http;

    public GitHubApiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _http.DefaultRequestHeaders.Add("User-Agent", "PortalEstudos");
        _http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");

        // Token opcional (GitHub:Token): eleva o rate limit de 60 para 5000 req/h na API pública.
        // Sem token o portal funciona normalmente (limite anônimo é suficiente p/ uso leve).
        var token = configuration["GitHub:Token"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IReadOnlyList<ChallengePrStatus>> GetChallengePrStatusAsync()
    {
        var result = new List<ChallengePrStatus>();
        try
        {
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
            // Falha silenciosa: o portal ainda mostra o desafio, só sem status de PR.
        }

        return result;
    }

    private static int? ExtractChallengeId(string title, string body)
    {
        // Procura "Desafio Semana N" ou "Semana-N" no título/corpo.
        var match = System.Text.RegularExpressions.Regex.Match(
            $"{title} {body}", @"[Ss]emana[-\s]?(\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, out int id) ? id : null;
    }

    private sealed class GitHubPull
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "";
        [JsonPropertyName("body")] public string? Body { get; set; }
        [JsonPropertyName("state")] public string State { get; set; } = "";
        [JsonPropertyName("merged")] public bool? Merged { get; set; }
        [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = "";
    }
}
