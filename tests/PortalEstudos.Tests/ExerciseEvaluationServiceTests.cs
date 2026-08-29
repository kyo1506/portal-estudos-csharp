using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using PortalEstudos.Domain.Enums;
using Xunit;

namespace PortalEstudos.Tests;

public class ExerciseEvaluationServiceTests
{
    private readonly ExerciseEvaluationService _service = new();

    private static ExerciseModel BuildExercise(string? solution) => new()
    {
        Id = 1,
        Title = "Ex",
        Description = "D",
        InitialCode = "using System;",
        ExpectedOutput = "out",
        Difficulty = ExerciseDifficulty.Easy,
        Hints = new List<string>(),
        Solution = solution
    };

    private const string SolutionCode =
        "using System;\npublic class Program {\n public static void Main() {\n   Console.WriteLine(\"Olá\");\n }\n}";

    [Fact]
    public void IsCorrect_EmptyCode_ReturnsFalse()
    {
        var exercise = BuildExercise(SolutionCode);
        Assert.False(_service.IsCorrect("", exercise));
        Assert.False(_service.IsCorrect("   ", exercise));
        Assert.False(_service.IsCorrect(null!, exercise));
    }

    [Fact]
    public void IsCorrect_NoSolution_RequiresOutput()
    {
        var exercise = BuildExercise(null);
        Assert.True(_service.IsCorrect("Console.WriteLine(\"x\");", exercise));
        Assert.False(_service.IsCorrect("var x = 1;", exercise));
    }

    [Fact]
    public void IsCorrect_ExactMatch_ReturnsTrue()
    {
        var exercise = BuildExercise(SolutionCode);
        Assert.True(_service.IsCorrect(SolutionCode, exercise));
    }

    [Fact]
    public void IsCorrect_WhitespaceOnlyDifference_ReturnsTrue()
    {
        var exercise = BuildExercise(SolutionCode);
        var variant = "using  System;\n\npublic class Program  {\n\npublic static void Main()   {\n\nConsole.WriteLine( \"Olá\" );\n\n}\n\n}";
        Assert.True(_service.IsCorrect(variant, exercise));
    }

    [Fact]
    public void IsCorrect_CommentDoesNotChangeVerdict()
    {
        var exercise = BuildExercise(SolutionCode);
        var commented = "// comentário\n" + SolutionCode;
        Assert.True(_service.IsCorrect(commented, exercise));
    }

    [Fact]
    public void IsCorrect_EquivalentLogicWithOutput_ReturnsTrue()
    {
        var exercise = BuildExercise(SolutionCode);
        // Diferente da solução exata, mas traz a lógica central (Program/Main/Console.WriteLine).
        var user = "using System;\nclass Program { static void Main() { Console.WriteLine(\"Olá\"); } }";
        Assert.True(_service.IsCorrect(user, exercise));
    }

    [Fact]
    public void IsCorrect_MissingOutput_ReturnsFalse()
    {
        var exercise = BuildExercise(SolutionCode);
        var user = "using System;\npublic class Program { public static void Main() { var x = 1; } }";
        Assert.False(_service.IsCorrect(user, exercise));
    }

    [Fact]
    public void IsCorrect_CompletelyDifferentCode_ReturnsFalse()
    {
        var exercise = BuildExercise(SolutionCode);
        var user = "int a = 1; int b = 2; int c = a + b;";
        Assert.False(_service.IsCorrect(user, exercise));
    }

    [Theory]
    [InlineData("public  class  Program", "public class Program")]
    [InlineData("//x\r\npublic class X", "public class X")]
    [InlineData("var s = \"a  b\";", "var s = \"\";")]
    [InlineData("a   b", "a b")]
    public void NormalizeCode_ExpectedNormalization(string input, string expected)
    {
        Assert.Equal(expected, ExerciseEvaluationService.NormalizeCode(input));
    }

    [Fact]
    public void ExtractTokens_ReturnsDistinctIdentifiers()
    {
        var tokens = ExerciseEvaluationService.ExtractTokens("public class Program Main WriteLine Console");
        Assert.Equal(new[] { "public", "class", "Program", "Main", "WriteLine", "Console" }, tokens);
    }
}
