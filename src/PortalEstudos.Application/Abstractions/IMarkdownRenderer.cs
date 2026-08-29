namespace PortalEstudos.Application.Abstractions;

/// <summary>Renderiza Markdown (HTML e texto puro).</summary>
public interface IMarkdownRenderer
{
    string ToHtml(string markdown);
    string ToPlainText(string markdown);
}
