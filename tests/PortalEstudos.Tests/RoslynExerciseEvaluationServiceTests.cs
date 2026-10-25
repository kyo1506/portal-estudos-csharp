namespace PortalEstudos.Tests;

using PortalEstudos.Domain.Entities;
using PortalEstudos.Domain.Enums;
using PortalEstudos.Infrastructure.Evaluation;
using Xunit;

public class RoslynExerciseEvaluationServiceTests
{
    private readonly RoslynExerciseEvaluationService _service = new();

    private static ExerciseModel CriarExercicio(string expectedOutput) => new()
    {
        Id = 1,
        Title = "Teste Par ou Impar",
        Description = "Verifica se numero eh par",
        InitialCode = "using System;",
        ExpectedOutput = expectedOutput,
        Difficulty = ExerciseDifficulty.Easy,
        Hints = new List<string>(),
        Solution = ""
    };

    [Fact]
    public async Task EvaluateAsync_CodigoValidoESaidaCorreta_RetornaSucesso()
    {
        var exercicio = CriarExercicio("True\nFalse");
        var userCode = @"
using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine(EhPar(4));
        Console.WriteLine(EhPar(7));
    }

    static bool EhPar(int n) => n % 2 == 0;
}
";

        var resultado = await _service.EvaluateAsync(userCode, exercicio);

        Assert.True(resultado.IsCorrect);
        Assert.Contains("True", resultado.ActualOutput);
        Assert.Contains("False", resultado.ActualOutput);
        Assert.Null(resultado.CompilerErrors);
    }

    [Fact]
    public async Task EvaluateAsync_ErroCompilacao_RetornaDiagnosticoRoslyn()
    {
        var exercicio = CriarExercicio("42");
        var userCodeComErroSintaxe = @"
using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine(""Sem ponto e virgula"")
    }
}
";

        var resultado = await _service.EvaluateAsync(userCodeComErroSintaxe, exercicio);

        Assert.False(resultado.IsCorrect);
        Assert.NotNull(resultado.CompilerErrors);
        Assert.Contains("CS1002", resultado.CompilerErrors);
        Assert.Equal("Erro de compilação C#.", resultado.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_ExcecaoRuntime_CapturaExcecao()
    {
        var exercicio = CriarExercicio("10");
        var userCodeComDivisaoZero = @"
using System;

public class Program
{
    public static void Main()
    {
        int a = 10;
        int b = 0;
        Console.WriteLine(a / b);
    }
}
";

        var resultado = await _service.EvaluateAsync(userCodeComDivisaoZero, exercicio);

        Assert.False(resultado.IsCorrect);
        Assert.Contains("DivideByZeroException", resultado.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_SaidaDivergente_RetornaFalhaComSaidaReal()
    {
        var exercicio = CriarExercicio("100");
        var userCodeComSaidaDiferente = @"
using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine(""200"");
    }
}
";

        var resultado = await _service.EvaluateAsync(userCodeComSaidaDiferente, exercicio);

        Assert.False(resultado.IsCorrect);
        Assert.Equal("200\n", resultado.ActualOutput.Replace("\r\n", "\n"));
        Assert.Contains("A saída do console foi diferente da saída esperada", resultado.ErrorMessage);
    }

    [Fact]
    public async Task EvaluateAsync_CodigoVazio_RetornaErro()
    {
        var exercicio = CriarExercicio("Qualquer");
        var resultado = await _service.EvaluateAsync("", exercicio);

        Assert.False(resultado.IsCorrect);
        Assert.Equal("Nenhum código fornecido para avaliação.", resultado.ErrorMessage);
    }
}
