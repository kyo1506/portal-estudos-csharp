using PortalEstudos.Models;

namespace PortalEstudos.Services;

public interface IContentService
{
    List<WeekModel> GetAllWeeks();
    WeekModel? GetWeek(int id);
    LessonModel? GetLesson(int weekId, int lessonId);
    ExerciseModel? GetExercise(int weekId, int exerciseId);
    DashboardStats GetDashboardStats(UserProgress progress);
}

public class ContentService : IContentService
{
    private readonly List<WeekModel> _weeks;

    public ContentService()
    {
        _weeks = InitializeContent();
    }

    public List<WeekModel> GetAllWeeks() => _weeks;

    public WeekModel? GetWeek(int id) => _weeks.FirstOrDefault(w => w.Id == id);

    public LessonModel? GetLesson(int weekId, int lessonId)
    {
        var week = GetWeek(weekId);
        return week?.Lessons.FirstOrDefault(l => l.Id == lessonId);
    }

    public ExerciseModel? GetExercise(int weekId, int exerciseId)
    {
        var week = GetWeek(weekId);
        return week?.Exercises.FirstOrDefault(e => e.Id == exerciseId);
    }

    public DashboardStats GetDashboardStats(UserProgress progress)
    {
        var totalLessons = _weeks.Sum(w => w.Lessons.Count);
        var totalExercises = _weeks.Sum(w => w.Exercises.Count);
        var totalChallenges = _weeks.Count(w => w.Challenge != null);

        return new DashboardStats
        {
            TotalWeeks = _weeks.Count,
            CompletedLessons = progress.CompletedLessons.Count,
            TotalLessons = totalLessons,
            CompletedExercises = progress.CompletedExercises.Count,
            TotalExercises = totalExercises,
            CompletedChallenges = progress.CompletedChallenges.Count,
            TotalChallenges = totalChallenges,
            CurrentStreak = progress.CurrentStreak,
            ProgressPercentage = totalLessons + totalExercises > 0
                ? Math.Round((double)(progress.CompletedLessons.Count + progress.CompletedExercises.Count) / (totalLessons + totalExercises) * 100, 1)
                : 0
        };
    }

    private List<WeekModel> InitializeContent()
    {
        return new List<WeekModel>
        {
            new WeekModel
            {
                Id = 1,
                Title = "Memória, Tipos e a Base de Tudo",
                Description = "Stack vs Heap, Tipos por Valor vs Referência, Boxing/Unboxing",
                Icon = "🧠",
                Lessons = new List<LessonModel>
                {
                    new LessonModel
                    {
                        Id = 1,
                        Title = "Tipos por Valor vs Tipos por Referência",
                        Content = @"## Tipos por Valor vs Tipos por Referência

Em C#, todo dado tem um **tipo** que define:
- Quanta memória ocupa
- Quais valores pode assumir
- Quais operações podem ser feitas

### Tipos por Valor (Value Types)
Guardam o valor diretamente na memória (stack):
- `int`, `double`, `bool`, `char`, `decimal`
- `struct`, `enum`
- `DateTime`

### Tipos por Referência (Reference Types)
Guardam o endereço do valor (heap):
- `class`, `string`, `array`
- `interface`, `delegate`

### Exemplo Prático

```csharp
// Struct = tipo por valor
public struct Dinheiro
{
    public decimal Valor;
    public string Moeda;
}

// Class = tipo por referência
public class Estoque
{
    public string Nome;
    public List<Produto> Produtos;
}
```",
                        CodeExample = @"using System;

public struct Dinheiro
{
    public decimal Valor;
    public string Moeda;
    
    public Dinheiro(decimal valor, string moeda = ""BRL"")
    {
        Valor = valor;
        Moeda = moeda;
    }
    
    public override string ToString() => $""{Moeda} {Valor:N2}"";
}

public class Program
{
    public static void Main()
    {
        // Tipo por valor: cópia independente
        var d1 = new Dinheiro(15.90m);
        var d2 = d1;  // COPIA o valor
        d2.Valor = 20.00m;
        
        Console.WriteLine($""d1: {d1}"");  // BRL 15,90
        Console.WriteLine($""d2: {d2}"");  // BRL 20,00
    }
}",
                        CodeLanguage = "csharp",
                        Order = 1
                    },
                    new LessonModel
                    {
                        Id = 2,
                        Title = "Stack vs Heap",
                        Content = @"## Stack vs Heap

### Stack (Pilha)
- Memória rápida e organizada
- Tamanho fixo por variável
- Alocação/desalocação automática
- Tipos por valor vivem aqui

### Heap (Monte)
- Memória dinâmica
- Objetos de classe vivem aqui
- Coletada pelo Garbage Collector
- Mais lenta que a stack

### Visualização

```
STACK                          HEAP
┌─────────────────┐           ┌──────────────────────┐
│ int x = 42      │           │ Dinheiro object      │
│ Dinheiro d1 ────│──────────→│ Valor: 15.90         │
│ (valor direto)  │           │ Moeda: BRL           │
└─────────────────┘           └──────────────────────┘
```",
                        CodeExample = @"// Demonstração de Stack vs Heap
public struct Point { public int X, Y; }

public class Program
{
    public static void Main()
    {
        // Stack: valor direto
        int numero = 42;
        Point p1 = new Point { X = 1, Y = 2 };
        
        // Heap: referência
        var texto = new string(""Hello"");
        var lista = new List<int> { 1, 2, 3 };
        
        Console.WriteLine($""int é valor: {numero}"");
        Console.WriteLine($""string é referência: {texto}"");
    }
}",
                        CodeLanguage = "csharp",
                        Order = 2
                    },
                    new LessonModel
                    {
                        Id = 3,
                        Title = "Boxing e Unboxing",
                        Content = @"## Boxing e Unboxing

### Boxing
É quando um tipo por valor é ""embalado"" para a heap (object).

```csharp
int numero = 42;
object obj = numero;  // Boxing! int → object
```

### Unboxing
É quando o valor é desembalado de volta.

```csharp
int deVolta = (int)obj;  // Unboxing
```

### Performance
Boxing/unboxing tem custo! Evite em loops.

```csharp
// ❌ Lento: boxing em cada iteração
ArrayList lista = new ArrayList();
for (int i = 0; i < 100000; i++)
    lista.Add(i);  // Boxing!

// ✅ Rápido: sem boxing
List<int> listaOtimizada = new List<int>();
for (int i = 0; i < 100000; i++)
    listaOtimizada.Add(i);  // Sem boxing
```",
                        CodeExample = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        var sw = Stopwatch.StartNew();
        
        // Com boxing
        var listaLenta = new ArrayList();
        for (int i = 0; i < 100000; i++)
            listaLenta.Add(i);
        sw.Stop();
        Console.WriteLine($""ArrayList (boxing): {sw.ElapsedMilliseconds}ms"");
        
        // Sem boxing
        sw.Restart();
        var listaRapida = new List<int>();
        for (int i = 0; i < 100000; i++)
            listaRapida.Add(i);
        sw.Stop();
        Console.WriteLine($""List<int>: {sw.ElapsedMilliseconds}ms"");
    }
}",
                        CodeLanguage = "csharp",
                        Order = 3
                    }
                },
                Exercises = new List<ExerciseModel>
                {
                    new ExerciseModel
                    {
                        Id = 1,
                        Title = "Criar struct Dinheiro",
                        Description = "Crie um struct chamado Dinheiro com propriedades Valor (decimal) e Moeda (string). Implemente um construtor e o método ToString().",
                        InitialCode = @"using System;

// Crie seu struct aqui


public class Program
{
    public static void Main()
    {
        var preco = new Dinheiro(15.90m, ""BRL"");
        Console.WriteLine(preco);
    }
}",
                        ExpectedOutput = "BRL 15,90",
                        Difficulty = ExerciseDifficulty.Easy,
                        Hints = new List<string> { "Use 'struct' em vez de 'class'", "ToString() pode usar interpolação de strings" },
                        Solution = @"public struct Dinheiro
{
    public decimal Valor { get; }
    public string Moeda { get; }
    
    public Dinheiro(decimal valor, string moeda = ""BRL"")
    {
        Valor = valor;
        Moeda = moeda;
    }
    
    public override string ToString() => $""{Moeda} {Valor:N2}"";
}"
                    },
                    new ExerciseModel
                    {
                        Id = 2,
                        Title = "Diferença entre cópia e referência",
                        Description = "Demonstre a diferença entre copiar um struct e copiar uma referência de classe.",
                        InitialCode = @"using System;

public struct Ponto { public int X, Y; }
public class Circulo { public int Raio; }

public class Program
{
    public static void Main()
    {
        // Demonstre a diferença aqui
        
    }
}",
                        ExpectedOutput = "p1.X = 1\r\np2.X = 10\r\nc1.Raio = 10\r\nc2.Raio = 10",
                        Difficulty = ExerciseDifficulty.Easy,
                        Hints = new List<string> { "Structs são copiados por valor", "Classes compartilham a referência" },
                        Solution = @"var p1 = new Ponto { X = 1, Y = 2 };
var p2 = p1;  // Cópia independente
p2.X = 10;

var c1 = new Circulo { Raio = 5 };
var c2 = c1;  // Mesma referência!
c2.Raio = 10;

Console.WriteLine($""p1.X = {p1.X}"");  // 1
Console.WriteLine($""p2.X = {p2.X}"");  // 10
Console.WriteLine($""c1.Raio = {c1.Raio}"");  // 10
Console.WriteLine($""c2.Raio = {c2.Raio}"");  // 10"
                    },
                    new ExerciseModel
                    {
                        Id = 3,
                        Title = "Performance: ArrayList vs List<T>",
                        Description = "Compare a performance entre ArrayList (com boxing) e List<int> (sem boxing) adicionando 100.000 elementos.",
                        InitialCode = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        // Meça o tempo de ambos
        
    }
}",
                        ExpectedOutput = "ArrayList: *ms\r\nList<int>: *ms",
                        Difficulty = ExerciseDifficulty.Medium,
                        Hints = new List<string> { "Use Stopwatch.StartNew()", "ArrayList faz boxing, List<int> não" },
                        Solution = @"var sw = Stopwatch.StartNew();
var arrayList = new ArrayList();
for (int i = 0; i < 100000; i++)
    arrayList.Add(i);
sw.Stop();
Console.WriteLine($""ArrayList: {sw.ElapsedMilliseconds}ms"");

sw.Restart();
var listInt = new List<int>();
for (int i = 0; i < 100000; i++)
    listInt.Add(i);
sw.Stop();
Console.WriteLine($""List<int>: {sw.ElapsedMilliseconds}ms"");"
                    }
                },
                Challenge = new ChallengeModel
                {
                    Id = 1,
                    Title = "Desafio Semana 1: Controle de Estoque",
                    Description = "Crie um sistema de controle de estoque completo usando structs para produtos e classes para o estoque.",
                    GitHubUrl = "https://github.com/kyo1506/fundamentos-csharp/tree/main/Semana-01/desafios-semanais",
                    Requirements = new List<string>
                    {
                        "Criar struct ProdutoPerecivel com Nome, CodigoBarras, Preco, QuantidadeEstoque",
                        "Criar classe Estoque com lista de produtos e métodos de entrada/saída",
                        "Implementar busca por código de barras",
                        "Usar ref/out para atualização em lote",
                        "Criar testes unitários"
                    }
                }
            },
            new WeekModel
            {
                Id = 2,
                Title = "Programação Orientada a Objetos",
                Description = "Encapsulamento, Herança, Polimorfismo, Abstração + SOLID",
                Icon = "🧩",
                Lessons = new List<LessonModel>
                {
                    new LessonModel
                    {
                        Id = 1,
                        Title = "Os 4 Pilares da OO",
                        Content = @"## Pilares da Orientação a Objetos

### 1. Encapsulamento
Esconder estado interno, expor apenas o necessário.

```csharp
public class ContaBancaria
{
    private decimal _saldo;  // Campo privado
    
    public decimal Saldo => _saldo;  // Propriedade somente leitura
    
    public void Depositar(decimal valor)
    {
        if (valor > 0) _saldo += valor;
    }
}
```

### 2. Herança
Reaproveitar código em hierarquias.

```csharp
public abstract class Animal
{
    public abstract void FazerSom();
}

public class Cachorro : Animal
{
    public override void FazerSom() => Console.WriteLine(""Au au!"");
}
```

### 3. Polimorfismo
Tratar objetos diferentes de forma uniforme.

```csharp
List<Animal> animais = new()
{
    new Cachorro(),
    new Gato()
};

foreach (var animal in animais)
    animal.FazerSom();  // Cada um faz seu som!
```

### 4. Abstração
Focar no essencial, ignorar detalhes.

```csharp
public interface IPagamento
{
    void Processar(decimal valor);
}

public class Pix : IPagamento { /* ... */ }
public class Cartao : IPagamento { /* ... */ }
```",
                        CodeExample = @"using System;
using System.Collections.Generic;

// Abstração
public interface IForma
{
    double CalcularArea();
}

// Implementação
public class Circulo : IForma
{
    public double Raio { get; set; }
    public double CalcularArea() => Math.PI * Raio * Raio;
}

public class Retangulo : IForma
{
    public double Largura { get; set; }
    public double Altura { get; set; }
    public double CalcularArea() => Largura * Altura;
}

public class Program
{
    public static void Main()
    {
        List<IForma> formas = new()
        {
            new Circulo { Raio = 5 },
            new Retangulo { Largura = 4, Altura = 6 }
        };
        
        foreach (var forma in formas)
            Console.WriteLine($""Área: {forma.CalcularArea():F2}"");
    }
}",
                        CodeLanguage = "csharp",
                        Order = 1
                    },
                    new LessonModel
                    {
                        Id = 2,
                        Title = "Princípios SOLID",
                        Content = @"## SOLID

### S - Single Responsibility Principle
Uma classe deve ter apenas UMA razão para mudar.

### O - Open/Closed Principle
Aberto para extensão, fechado para modificação.

### L - Liskov Substitution Principle
Derivadas devem poder substituir bases.

### I - Interface Segregation Principle
Interfaces pequenas > uma interface grande.

### D - Dependency Inversion Principle
Dependa de abstrações, não de implementações.",
                        CodeExample = @"using System;

// SRP: Cada classe tem uma responsabilidade
public class ValidadorCpf
{
    public bool Validar(string cpf) => cpf.Length == 11;
}

// OCP: Extensão sem modificar
public interface IDesconto
{
    decimal Calcular(decimal valor);
}

public class DescontoEstudante : IDesconto
{
    public decimal Calcular(decimal valor) => valor * 0.5m;
}

// DIP: Depende de abstração
public class ServicoInscricao
{
    private readonly IDesconto _desconto;
    
    public ServicoInscricao(IDesconto desconto)
    {
        _desconto = desconto;
    }
    
    public decimal CalcularPreco(decimal precoBase)
    {
        return _desconto.Calcular(precoBase);
    }
}

public class Program
{
    public static void Main()
    {
        var servico = new ServicoInscricao(new DescontoEstudante());
        Console.WriteLine($""Preço com desconto: {servico.CalcularPreco(100m):C}"");
    }
}",
                        CodeLanguage = "csharp",
                        Order = 2
                    }
                },
                Exercises = new List<ExerciseModel>
                {
                    new ExerciseModel
                    {
                        Id = 1,
                        Title = "Encapsulamento com validação",
                        Description = "Crie uma classe Produto com propriedades validadas no setter.",
                        InitialCode = @"using System;

// Crie a classe Produto com validação


public class Program
{
    public static void Main()
    {
        var p = new Produto { Nome = ""Arroz"", Preco = 15.90m };
        Console.WriteLine($""{p.Nome}: {p.Preco:C}"");
        
        // Deve lançar exceção
        try { p.Preco = -10m; }
        catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
    }
}",
                        ExpectedOutput = "Arroz: R$ 15,90\r\nPreço deve ser maior que zero",
                        Difficulty = ExerciseDifficulty.Easy,
                        Hints = new List<string> { "Use backing field privado", "Valide no setter antes de atribuir" },
                        Solution = @"public class Produto
{
    private string _nome = """";
    private decimal _preco;
    
    public string Nome
    {
        get => _nome;
        set => _nome = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(""Nome não pode ser vazio"")
            : value;
    }
    
    public decimal Preco
    {
        get => _preco;
        set => _preco = value <= 0
            ? throw new ArgumentException(""Preço deve ser maior que zero"")
            : value;
    }
}"
                    },
                    new ExerciseModel
                    {
                        Id = 2,
                        Title = "Herança e Polimorfismo",
                        Description = "Crie uma hierarquia de formas geométricas com polimorfismo.",
                        InitialCode = @"using System;
using System.Collections.Generic;

// Crie as classes aqui


public class Program
{
    public static void Main()
    {
        List<Forma> formas = new()
        {
            new Circulo { Raio = 5 },
            new Retangulo { Largura = 4, Altura = 6 }
        };
        
        foreach (var f in formas)
            Console.WriteLine($""Área: {f.CalcularArea():F2}"");
    }
}",
                        ExpectedOutput = "Área: 78,54\r\nÁrea: 24,00",
                        Difficulty = ExerciseDifficulty.Medium,
                        Hints = new List<string> { "Use classe abstrata ou interface", "Cada forma calcula área diferente" },
                        Solution = @"public abstract class Forma
{
    public abstract double CalcularArea();
}

public class Circulo : Forma
{
    public double Raio { get; set; }
    public override double CalcularArea() => Math.PI * Raio * Raio;
}

public class Retangulo : Forma
{
    public double Largura { get; set; }
    public double Altura { get; set; }
    public override double CalcularArea() => Largura * Altura;
}"
                    }
                },
                Challenge = new ChallengeModel
                {
                    Id = 2,
                    Title = "Desafio Semana 2: Validação de Formulários",
                    Description = "Crie um sistema de validação de formulários usando interfaces e SOLID.",
                    GitHubUrl = "https://github.com/kyo1506/fundamentos-csharp/tree/main/Semana-02/desafios-semanais",
                    Requirements = new List<string>
                    {
                        "Interface IValidador com método Validar()",
                        "Validadores separados: CPF, Email, Telefone",
                        "Classe Formulario com lista de validadores",
                        "Aplicar SRP, OCP e DIP",
                        "Testes unitários"
                    }
                }
            },
            new WeekModel
            {
                Id = 3,
                Title = "Coleções, LINQ e Manipulação de Dados",
                Description = "List<T>, Dictionary, GroupBy, Where, OrderBy",
                Icon = "📊",
                Lessons = new List<LessonModel>
                {
                    new LessonModel
                    {
                        Id = 1,
                        Title = "Coleções Genéricas",
                        Content = @"## Coleções em C#

### List<T>
Lista indexada, permite duplicatas.

```csharp
List<string> nomes = new();
nomes.Add(""Maria"");
nomes.Add(""João"");
```

### Dictionary<TKey, TValue>
Pares chave-valor, busca O(1).

```csharp
Dictionary<int, string> catalogo = new();
catalogo[1] = ""Arroz"";
```

### HashSet<T>
Conjunto sem duplicatas.

```csharp
HashSet<string> tags = new() { ""C#"", "".NET"" };
```",
                        CodeExample = @"using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        // List<T>
        var produtos = new List<string> { ""Arroz"", ""Feijão"", ""Arroz"" };
        Console.WriteLine($""List: {produtos.Count} itens"");  // 3
        
        // Dictionary<K,V>
        var precos = new Dictionary<string, decimal>
        {
            [""Arroz""] = 15.90m,
            [""Feijão""] = 8.50m
        };
        Console.WriteLine($""Arroz: {precos[""Arroz""]:C}"");
        
        // HashSet<T>
        var categorias = new HashSet<string> { ""Alimentos"", ""Alimentos"" };
        Console.WriteLine($""HashSet: {categorias.Count} itens"");  // 1
    }
}",
                        CodeLanguage = "csharp",
                        Order = 1
                    },
                    new LessonModel
                    {
                        Id = 2,
                        Title = "LINQ - Consultas Declarativas",
                        Content = @"## LINQ

### Filtrar com Where
```csharp
var baratos = produtos.Where(p => p.Preco < 50);
```

### Projetar com Select
```csharp
var nomes = produtos.Select(p => p.Nome);
```

### Agrupar com GroupBy
```csharp
var porCategoria = produtos.GroupBy(p => p.Categoria);
```

### Ordenar com OrderBy
```csharp
var ordenados = produtos.OrderBy(p => p.Preco);
```",
                        CodeExample = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var produtos = new List<(string Nome, string Cat, decimal Preco)>
        {
            (""Arroz"", ""Alimentos"", 15.90m),
            (""Notebook"", ""Eletrônicos"", 3500m),
            (""Feijão"", ""Alimentos"", 8.50m),
            (""Mouse"", ""Eletrônicos"", 89.90m)
        };
        
        // Where + OrderBy
        var baratos = produtos
            .Where(p => p.Preco < 100)
            .OrderBy(p => p.Preco);
        
        foreach (var p in baratos)
            Console.WriteLine($""{p.Nome}: {p.Preco:C}"");
        
        // GroupBy
        var grupos = produtos.GroupBy(p => p.Cat);
        foreach (var g in grupos)
            Console.WriteLine($""{g.Key}: {g.Count()} itens"");
    }
}",
                        CodeLanguage = "csharp",
                        Order = 2
                    }
                },
                Exercises = new List<ExerciseModel>
                {
                    new ExerciseModel
                    {
                        Id = 1,
                        Title = "Filtrar e ordenar com LINQ",
                        Description = "Dada uma lista de produtos, filtre os que custam menos de 100 e ordene por preço.",
                        InitialCode = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var produtos = new List<(string Nome, decimal Preco)>
        {
            (""Arroz"", 15.90m),
            (""Notebook"", 3500m),
            (""Feijão"", 8.50m),
            (""Mouse"", 89.90m)
        };
        
        // Filtre e ordene aqui
        
    }
}",
                        ExpectedOutput = "Feijão: R$ 8,50\r\nArroz: R$ 15,90\r\nMouse: R$ 89,90",
                        Difficulty = ExerciseDifficulty.Easy,
                        Hints = new List<string> { "Use Where para filtrar", "Use OrderBy para ordenar" },
                        Solution = @"var baratos = produtos
    .Where(p => p.Preco < 100)
    .OrderBy(p => p.Preco);

foreach (var p in baratos)
    Console.WriteLine($""{p.Nome}: {p.Preco:C}"");"
                    },
                    new ExerciseModel
                    {
                        Id = 2,
                        Title = "Agrupar com GroupBy",
                        Description = "Agrupe produtos por categoria e calcule o total por categoria.",
                        InitialCode = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var produtos = new List<(string Nome, string Categoria, decimal Preco)>
        {
            (""Arroz"", ""Alimentos"", 15.90m),
            (""Feijão"", ""Alimentos"", 8.50m),
            (""Notebook"", ""Eletrônicos"", 3500m),
            (""Mouse"", ""Eletrônicos"", 89.90m)
        };
        
        // Agrupe por categoria
        
    }
}",
                        ExpectedOutput = "Alimentos: R$ 24,40\r\nEletrônicos: R$ 3.589,90",
                        Difficulty = ExerciseDifficulty.Medium,
                        Hints = new List<string> { "Use GroupBy(p => p.Categoria)", "Use Sum para totalizar" },
                        Solution = @"var grupos = produtos.GroupBy(p => p.Categoria);

foreach (var g in grupos)
{
    decimal total = g.Sum(p => p.Preco);
    Console.WriteLine($""{g.Key}: {total:C}"");
}"
                    }
                },
                Challenge = new ChallengeModel
                {
                    Id = 3,
                    Title = "Desafio Semana 3: Análise de Vendas",
                    Description = "Crie um sistema de análise de vendas com LINQ.",
                    GitHubUrl = "https://github.com/kyo1506/fundamentos-csharp/tree/main/Semana-03/desafios-semanais",
                    Requirements = new List<string>
                    {
                        "Gerar 50 vendas aleatórias",
                        "Filtrar por categoria e período",
                        "Agrupar por mês e categoria",
                        "Calcular ticket médio",
                        "Top 5 produtos mais vendidos"
                    }
                }
            },
            new WeekModel
            {
                Id = 4,
                Title = "ASP.NET Core e Deploy",
                Description = "Minimal APIs, Razor Pages, Deploy em nuvem",
                Icon = "🚀",
                Lessons = new List<LessonModel>
                {
                    new LessonModel
                    {
                        Id = 1,
                        Title = "Criando uma Minimal API",
                        Content = @"## Minimal APIs

Forma mais simples de criar APIs no .NET.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet(""/produtos"", () => ""Lista de produtos"");
app.MapGet(""/produtos/{id}"", (int id) => $""Produto {id}"");
app.MapPost(""/produtos"", () => ""Produto criado"");

app.Run();
```",
                        CodeExample = @"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// GET /produtos
app.MapGet(""/produtos"", () =>
{
    var produtos = new[]
    {
        new { Id = 1, Nome = ""Arroz"" },
        new { Id = 2, Nome = ""Feijão"" }
    };
    return Results.Ok(produtos);
});

// POST /produtos
app.MapPost(""/produtos"", (ProdutoRequest req) =>
{
    return Results.Created(""/produtos/1"", new { Id = 1, req.Nome });
});

app.Run();

public record ProdutoRequest(string Nome, decimal Preco);",
                        CodeLanguage = "csharp",
                        Order = 1
                    },
                    new LessonModel
                    {
                        Id = 2,
                        Title = "Deploy em Nuvem",
                        Content = @"## Deploy

### Opções Gratuitas
- **Azure Functions**: 1M req/mês
- **Railway**: $5 créditos/mês
- **Render**: 750h/mês

### CI/CD com GitHub Actions
```yaml
name: Deploy
on:
  push:
    branches: [main]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet build
      - run: dotnet test
      - run: dotnet publish -c Release
```",
                        CodeExample = @"// Dockerfile para deploy
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [""dotnet"", ""MinhaApi.dll""]",
                        CodeLanguage = "dockerfile",
                        Order = 2
                    }
                },
                Exercises = new List<ExerciseModel>
                {
                    new ExerciseModel
                    {
                        Id = 1,
                        Title = "Criar endpoint GET",
                        Description = "Crie uma Minimal API com um endpoint GET que retorna uma lista de produtos.",
                        InitialCode = @"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Crie seu endpoint aqui


app.Run();",
                        ExpectedOutput = "GET /produtos -> [{\"id\":1,\"nome\":\"Arroz\"}]",
                        Difficulty = ExerciseDifficulty.Easy,
                        Hints = new List<string> { "Use app.MapGet()", "Retorne Results.Ok()" },
                        Solution = @"app.MapGet(""/produtos"", () =>
{
    var produtos = new[]
    {
        new { Id = 1, Nome = ""Arroz"" },
        new { Id = 2, Nome = ""Feijão"" }
    };
    return Results.Ok(produtos);
});"
                    },
                    new ExerciseModel
                    {
                        Id = 2,
                        Title = "Endpoint com validação",
                        Description = "Crie um endpoint POST que valida os dados recebidos.",
                        InitialCode = @"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Crie o endpoint POST com validação


app.Run();

public record ProdutoRequest(string Nome, decimal Preco);",
                        ExpectedOutput = "POST /produtos (válido) -> 201\r\nPOST /produtos (inválido) -> 400",
                        Difficulty = ExerciseDifficulty.Medium,
                        Hints = new List<string> { "Use app.MapPost()", "Valide e retorne BadRequest se inválido" },
                        Solution = @"app.MapPost(""/produtos"", (ProdutoRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome))
        return Results.BadRequest(""Nome é obrigatório"");
    
    if (req.Preco <= 0)
        return Results.BadRequest(""Preço deve ser maior que zero"");
    
    return Results.Created(""/produtos/1"", new { Id = 1, req.Nome });
});"
                    }
                },
                Challenge = new ChallengeModel
                {
                    Id = 4,
                    Title = "Desafio Semana 4: API de Controle de Estoque",
                    Description = "Crie uma API RESTful completa com CRUD de produtos.",
                    GitHubUrl = "https://github.com/kyo1506/fundamentos-csharp/tree/main/Semana-04/desafios-semanais",
                    Requirements = new List<string>
                    {
                        "Endpoints GET, POST, PUT, DELETE",
                        "Validação de dados",
                        "Persistência em JSON ou memória",
                        "Swagger/OpenAPI",
                        "Deploy em nuvem gratuita"
                    }
                }
            }
        };
    }
}
