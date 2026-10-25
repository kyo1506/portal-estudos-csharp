namespace PortalEstudos.Infrastructure.Evaluation;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;

public sealed class RoslynExerciseEvaluationService : IExerciseEvaluationService
{
    private static readonly Lazy<List<MetadataReference>> LazyReferences = new(ObterReferenciasPadrao);

    public bool IsCorrect(string userCode, ExerciseModel exercise)
    {
        return EvaluateAsync(userCode, exercise).GetAwaiter().GetResult().IsCorrect;
    }

    public async Task<ExerciseEvaluationResult> EvaluateAsync(string userCode, ExerciseModel exercise, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return new ExerciseEvaluationResult(false, string.Empty, "Nenhum código fornecido para avaliação.", null);
        }

        var syntaxTree = CSharpSyntaxTree.ParseText(
            userCode,
            new CSharpParseOptions(LanguageVersion.Latest),
            cancellationToken: ct
        );

        var compilation = CSharpCompilation.Create(
            assemblyName: $"Exercise_{Guid.NewGuid():N}",
            syntaxTrees: new[] { syntaxTree },
            references: LazyReferences.Value,
            options: new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,
                concurrentBuild: true,
                allowUnsafe: false
            )
        );

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms, cancellationToken: ct);

        if (!emitResult.Success)
        {
            var erros = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d =>
                {
                    var lineSpan = d.Location.GetLineSpan();
                    var linha = lineSpan.StartLinePosition.Line + 1;
                    var coluna = lineSpan.StartLinePosition.Character + 1;
                    return $"Linha {linha}, Coluna {coluna}: {d.GetMessage()} ({d.Id})";
                });

            var compilerOutput = string.Join(Environment.NewLine, erros);
            return new ExerciseEvaluationResult(false, string.Empty, "Erro de compilação C#.", compilerOutput);
        }

        ms.Seek(0, SeekOrigin.Begin);
        var loadContext = new AssemblyLoadContext($"Runner_{Guid.NewGuid():N}", isCollectible: true);

        try
        {
            var assembly = loadContext.LoadFromStream(ms);
            var entryPoint = assembly.EntryPoint;

            if (entryPoint == null)
            {
                var programType = assembly.GetTypes().FirstOrDefault(t => t.Name == "Program");
                entryPoint = programType?.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            if (entryPoint == null)
            {
                return new ExerciseEvaluationResult(
                    false,
                    string.Empty,
                    "Ponto de entrada não localizado. Certifique-se de declarar 'public class Program { public static void Main() { ... } }'.",
                    null
                );
            }

            var oldOut = Console.Out;
            using var stringWriter = new StringWriter();
            string actualOutput;

            try
            {
                Console.SetOut(stringWriter);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                var executionTask = Task.Run(() =>
                {
                    var parameters = entryPoint.GetParameters();
                    object?[] args = parameters.Length == 0 ? Array.Empty<object?>() : new object?[] { Array.Empty<string>() };

                    var result = entryPoint.Invoke(null, args);
                    if (result is Task task)
                    {
                        task.GetAwaiter().GetResult();
                    }
                }, cts.Token);

                await executionTask;
                actualOutput = stringWriter.ToString();
            }
            catch (OperationCanceledException)
            {
                return new ExerciseEvaluationResult(
                    false,
                    stringWriter.ToString(),
                    "Tempo limite excedido (3 segundos). Verifique se há loops infinitos no código.",
                    null
                );
            }
            catch (TargetInvocationException ex)
            {
                var inner = ex.InnerException ?? ex;
                return new ExerciseEvaluationResult(
                    false,
                    stringWriter.ToString(),
                    $"Exceção em tempo de execução: {inner.GetType().Name}: {inner.Message}",
                    null
                );
            }
            catch (Exception ex)
            {
                return new ExerciseEvaluationResult(
                    false,
                    stringWriter.ToString(),
                    $"Falha ao executar o programa: {ex.Message}",
                    null
                );
            }
            finally
            {
                Console.SetOut(oldOut);
            }

            var normalizadaAtual = NormalizarTexto(actualOutput);
            var normalizadaEsperada = NormalizarTexto(exercise.ExpectedOutput);

            var isCorrect = string.Equals(normalizadaAtual, normalizadaEsperada, StringComparison.Ordinal);
            var msg = isCorrect ? null : "A saída do console foi diferente da saída esperada para o exercício.";

            return new ExerciseEvaluationResult(isCorrect, actualOutput, msg, null);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        var lines = texto.Replace("\r\n", "\n").Split('\n');
        var trimmedLines = lines.Select(l => l.TrimEnd());
        return string.Join("\n", trimmedLines).Trim();
    }

    private static List<MetadataReference> ObterReferenciasPadrao()
    {
        var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location) && File.Exists(asm.Location))
            {
                locations.Add(asm.Location);
            }
        }

        return locations
            .Select(loc => (MetadataReference)MetadataReference.CreateFromFile(loc))
            .ToList();
    }
}
