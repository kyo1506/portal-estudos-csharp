using System.Security.Cryptography;
using System.Text;
using Markdig;
using Microsoft.Extensions.Caching.Memory;
using PortalEstudos.Application.Abstractions;

namespace PortalEstudos.Infrastructure.Markdown;

/// <summary>Renderiza Markdown com Markdig, reutilizando o pipeline e cacheando o
/// resultado por hash do conteúdo (evita recálculo a cada renderização).</summary>
public sealed class MarkdigMarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private readonly IMemoryCache _cache;

    public MarkdigMarkdownRenderer(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return string.Empty;
        return _cache.GetOrCreate(Key("html", markdown), _ => Markdig.Markdown.ToHtml(markdown, Pipeline))!;
    }

    public string ToPlainText(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return string.Empty;
        return _cache.GetOrCreate(Key("text", markdown), _ => Markdig.Markdown.ToPlainText(markdown))!;
    }

    private static string Key(string kind, string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(kind + "|" + content));
        return $"md:{kind}:{Convert.ToHexString(bytes)}";
    }
}
