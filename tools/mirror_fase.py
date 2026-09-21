#!/usr/bin/env python3
"""
Espelha o conteúdo das Fases do repositorio 'fundamentos-csharp' para o
ContentSeed.json do portal (formato proprio do portal: licoes em Markdown +
exercicios interativos com solucao/esperado para o avaliador no browser).

Uso:
    python tools/mirror_fase.py --repo <caminho-fundamentos> --fase 0
    python tools/mirror_fase.py --repo <caminho-fundamentos> --fase 1
    python tools/mirror_fase.py --repo <caminho-fundamentos> --fase all

Convencao de pasta: <repo>/Fase-NN-Nome/teoria/*.md  (NN em 2 digitos, ex.: 00)
"""
import argparse, json, pathlib, re, sys

# ----- Exercicios interativos por numero de fase -----
# difficulty: 0=Easy, 1=Medium, 2=Hard
EXERCISES = {
    0: [
        dict(id=1, title="Par ou ímpar", difficulty=0,
             description="Escreva o método `EhPar(int numero)` que retorna `true` quando o número é par e `false` caso contrário.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(EhPar(7));\n        Console.WriteLine(EhPar(8));\n    }\n\n    // TODO: implemente EhPar aqui\n\n}",
             expectedOutput="False\r\nTrue",
             hints=["Use o operador resto `%`.", "Número par tem resto 0 na divisão por 2."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(EhPar(7));\n        Console.WriteLine(EhPar(8));\n    }\n\n    static bool EhPar(int numero)\n    {\n        return numero % 2 == 0;\n    }\n}"),
        dict(id=2, title="Classificar média", difficulty=0,
             description="Escreva `ClassificarMedia(double media)` que retorna \"Aprovado\" se média >= 6, senão \"Reprovado\".",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ClassificarMedia(7.5));\n        Console.WriteLine(ClassificarMedia(4.0));\n    }\n\n    // TODO: implemente ClassificarMedia aqui\n\n}",
             expectedOutput="Aprovado\r\nReprovado",
             hints=["Use `if` ou uma expressão `?:`.", "Compare a média com 6."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ClassificarMedia(7.5));\n        Console.WriteLine(ClassificarMedia(4.0));\n    }\n\n    static string ClassificarMedia(double media)\n    {\n        return media >= 6 ? \"Aprovado\" : \"Reprovado\";\n    }\n}"),
        dict(id=3, title="Número primo", difficulty=1,
             description="Escreva `EhPrimo(int n)` que retorna `true` se `n` é primo (maior que 1 e divisível só por 1 e por ele mesmo).",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(EhPrimo(7));\n        Console.WriteLine(EhPrimo(10));\n    }\n\n    // TODO: implemente EhPrimo aqui\n\n}",
             expectedOutput="True\r\nFalse",
             hints=["Números < 2 não são primos.", "Teste divisores de 2 até a raiz quadrada de n."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(EhPrimo(7));\n        Console.WriteLine(EhPrimo(10));\n    }\n\n    static bool EhPrimo(int n)\n    {\n        if (n < 2) return false;\n        for (int i = 2; i * i <= n; i++)\n            if (n % i == 0) return false;\n        return true;\n    }\n}"),
        dict(id=4, title="Fibonacci", difficulty=1,
             description="Escreva `Fibonacci(int n)` que devolve o n-ésimo termo (Fibonacci(0)=0, Fibonacci(1)=1).",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(Fibonacci(6));\n        Console.WriteLine(Fibonacci(10));\n    }\n\n    // TODO: implemente Fibonacci aqui\n\n}",
             expectedOutput="8\r\n55",
             hints=["Use um laço com duas variáveis anteriores.", "Cada termo é a soma dos dois anteriores."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(Fibonacci(6));\n        Console.WriteLine(Fibonacci(10));\n    }\n\n    static long Fibonacci(int n)\n    {\n        if (n <= 1) return n;\n        long a = 0, b = 1;\n        for (int i = 2; i <= n; i++)\n        {\n            long p = a + b; a = b; b = p;\n        }\n        return b;\n    }\n}"),
        dict(id=5, title="FizzBuzz", difficulty=0,
             description="Escreva `FizzBuzz(int n)` que retorna \"FizzBuzz\" se múltiplo de 3 e 5, \"Fizz\" se só de 3, \"Buzz\" se só de 5, senão o número como texto.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        for (int i = 1; i <= 15; i++)\n            Console.WriteLine(FizzBuzz(i));\n    }\n\n    // TODO: implemente FizzBuzz aqui\n\n}",
             expectedOutput="1\r\n2\r\nFizz\r\n4\r\nBuzz\r\nFizz\r\n7\r\n8\r\nFizz\r\nBuzz\r\n11\r\nFizz\r\n13\r\n14\r\nFizzBuzz",
             hints=["Use `%` para checar múltiplos.", "Teste múltiplo de 3 E 5 primeiro."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        for (int i = 1; i <= 15; i++)\n            Console.WriteLine(FizzBuzz(i));\n    }\n\n    static string FizzBuzz(int n)\n    {\n        if (n % 15 == 0) return \"FizzBuzz\";\n        if (n % 3 == 0) return \"Fizz\";\n        if (n % 5 == 0) return \"Buzz\";\n        return n.ToString();\n    }\n}"),
        dict(id=6, title="Soma dos dígitos", difficulty=1,
             description="Escreva `SomarDigitos(int n)` que soma os dígitos de um inteiro positivo (ex.: 123 → 6).",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SomarDigitos(123));\n        Console.WriteLine(SomarDigitos(9999));\n    }\n\n    // TODO: implemente SomarDigitos aqui\n\n}",
             expectedOutput="6\r\n36",
             hints=["`% 10` pega o último dígito.", "`/ 10` descarta o último dígito."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SomarDigitos(123));\n        Console.WriteLine(SomarDigitos(9999));\n    }\n\n    static int SomarDigitos(int n)\n    {\n        int soma = 0;\n        while (n > 0)\n        {\n            soma += n % 10;\n            n /= 10;\n        }\n        return soma;\n    }\n}"),
    ],
    1: [
        dict(id=1, title="Struct Dinheiro (valor e formato)", difficulty=0,
             description="Crie um `readonly struct Dinheiro` com propriedades `decimal Valor` e `string Moeda` (\"BRL\" por padrão). Implemente construtor e `ToString()` formatado.",
             initialCode="using System;\n\n// TODO: crie o readonly struct Dinheiro aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var d = new Dinheiro(15.90m, \"BRL\");\n        Console.WriteLine(d);\n    }\n}",
             expectedOutput="BRL 15,90",
             hints=["Use `public readonly struct Dinheiro`.", "Formate o valor com `{Valor:N2}` no ToString()."],
             solution="using System;\n\npublic readonly struct Dinheiro\n{\n    public decimal Valor { get; }\n    public string Moeda { get; }\n    public Dinheiro(decimal valor, string moeda = \"BRL\")\n    {\n        Valor = valor;\n        Moeda = moeda;\n    }\n    public override string ToString() => $\"{Moeda} {Valor:N2}\";\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var d = new Dinheiro(15.90m, \"BRL\");\n        Console.WriteLine(d);\n    }\n}"),
        dict(id=2, title="Inverter texto com StringBuilder", difficulty=0,
             description="Implemente a função `Inverter(string s)` utilizando `StringBuilder` sem usar LINQ.",
             initialCode="using System;\nusing System.Text;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(Inverter(\"dotnet\"));\n    }\n\n    // TODO: implemente Inverter aqui\n\n}",
             expectedOutput="tentod",
             hints=["Crie `new StringBuilder(s.Length)`.", "Percorra a string de trás para frente."],
             solution="using System;\nusing System.Text;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(Inverter(\"dotnet\"));\n    }\n\n    static string Inverter(string s)\n    {\n        var sb = new StringBuilder(s.Length);\n        for (int i = s.Length - 1; i >= 0; i--)\n            sb.Append(s[i]);\n        return sb.ToString();\n    }\n}"),
        dict(id=3, title="Divisão exata de parcelas", difficulty=1,
             description="Implemente `DividirParcelas(decimal total, int n)` que divide o valor em parcelas de centavos exatos, distribuindo sobras na primeira parcela.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p = DividirParcelas(100.00m, 3);\n        Console.WriteLine($\"{p[0]} + {p[1]} + {p[2]} = {p[0]+p[1]+p[2]}\");\n    }\n\n    // TODO: implemente DividirParcelas aqui\n\n}",
             expectedOutput="33,34 + 33,33 + 33,33 = 100,00",
             hints=["Use divisão inteira para centavos: `(int)(total * 100) / n`.", "O resto de centavos vai para a primeira parcela."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p = DividirParcelas(100.00m, 3);\n        Console.WriteLine($\"{p[0]:N2} + {p[1]:N2} + {p[2]:N2} = {p[0]+p[1]+p[2]:N2}\");\n    }\n\n    static decimal[] DividirParcelas(decimal total, int n)\n    {\n        decimal valorBase = Math.Floor((total / n) * 100m) / 100m;\n        decimal resto = total - (valorBase * n);\n        var res = new decimal[n];\n        for (int i = 0; i < n; i++)\n        {\n            res[i] = valorBase + (resto > 0 ? 0.01m : 0m);\n            if (resto > 0) resto -= 0.01m;\n        }\n        return res;\n    }\n}"),
        dict(id=4, title="Pattern matching de descontos", difficulty=1,
             description="Implemente `CalcularDesconto(decimal valor, bool vip)` usando switch expression com tupla. Regras: VIP com valor >= 1000 dá 25%; VIP dá 15%; valor >= 500 dá 5%; senão 0%.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(CalcularDesconto(1200m, true));\n        Console.WriteLine(CalcularDesconto(300m, true));\n        Console.WriteLine(CalcularDesconto(200m, false));\n    }\n\n    // TODO: implemente CalcularDesconto aqui\n\n}",
             expectedOutput="0,25\r\n0,15\r\n0,00",
             hints=["Use `(valor, vip) switch { ... }`.", "Lembre-se da ordem: a regra mais específica vem antes."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(CalcularDesconto(1200m, true).ToString(\"N2\"));\n        Console.WriteLine(CalcularDesconto(300m, true).ToString(\"N2\"));\n        Console.WriteLine(CalcularDesconto(200m, false).ToString(\"N2\"));\n    }\n\n    static decimal CalcularDesconto(decimal valor, bool vip) => (valor, vip) switch\n    {\n        (>= 1000m, true) => 0.25m,\n        (_, true)        => 0.15m,\n        (>= 500m, false) => 0.05m,\n        _                => 0.00m\n    };\n}"),
        dict(id=5, title="Método de extensão Truncar", difficulty=0,
             description="Crie uma classe estática `Extensoes` com o método de extensão `Truncar(this string texto, int max)` que adiciona \"...\" caso exceda o limite.",
             initialCode="using System;\n\n// TODO: declare a classe estática Extensoes aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        string txt = \"Aprender C# em profundidade\";\n        Console.WriteLine(txt.Truncar(10));\n    }\n}",
             expectedOutput="Aprender C...",
             hints=["A classe e o método devem ser `static`.", "O primeiro parâmetro deve usar o modificador `this`."],
             solution="using System;\n\npublic static class Extensoes\n{\n    public static string Truncar(this string texto, int max)\n    {\n        if (string.IsNullOrEmpty(texto) || texto.Length <= max) return texto;\n        return texto.Substring(0, max) + \"...\";\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        string txt = \"Aprender C# em profundidade\";\n        Console.WriteLine(txt.Truncar(10));\n    }\n}"),
        dict(id=6, title="Calcular dias úteis com DateOnly", difficulty=1,
             description="Implemente `ContarDiasUteis(DateOnly inicio, DateOnly fim)` que conta apenas de segunda a sexta entre duas datas inclusivas.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var d1 = new DateOnly(2026, 9, 14); // Segunda\n        var d2 = new DateOnly(2026, 9, 20); // Domingo\n        Console.WriteLine(ContarDiasUteis(d1, d2));\n    }\n\n    // TODO: implemente ContarDiasUteis aqui\n\n}",
             expectedOutput="5",
             hints=["Use um laço `while (atual <= fim)`.", "Verifique se `DayOfWeek` não é `Saturday` nem `Sunday`."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var d1 = new DateOnly(2026, 9, 14);\n        var d2 = new DateOnly(2026, 9, 20);\n        Console.WriteLine(ContarDiasUteis(d1, d2));\n    }\n\n    static int ContarDiasUteis(DateOnly inicio, DateOnly fim)\n    {\n        int dias = 0;\n        for (var d = inicio; d <= fim; d = d.AddDays(1))\n        {\n            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)\n                dias++;\n        }\n        return dias;\n    }\n}"),
    ],
}

FASE_META = {
    0: dict(id=0, title="Fundamentos da Programação (lógica com C#)",
            description="Do zero: variáveis e tipos, operadores, decisões, laços, arrays, strings, funções e lógica.",
            icon="🐣", challenge=None),
    1: dict(id=1, title="C# Básico: a linguagem em profundidade",
            description="Tipos por valor vs referência, strings/StringBuilder, decimal, coleções, exceções, structs/records e pattern matching.",
            icon="⚙️", challenge=None),
}


def ordem_numerica(name: str):
    m = re.match(r"(\d+)\.(\d+)", name)
    return (int(m.group(1)), int(m.group(2))) if m else (999, 0)


def carregar_fase(repo_path: pathlib.Path, num_fase: int):
    fase_dir = [p for p in repo_path.glob("Fase-*") if p.is_dir() and f"-{num_fase:02d}" in p.name]
    if not fase_dir:
        raise FileNotFoundError(f"pasta da Fase-{num_fase:02d} nao encontrada em {repo_path}")
    teoria_dir = fase_dir[0] / "teoria"

    lessons, order = [], 0
    for md in sorted(teoria_dir.glob("*.md"), key=lambda p: ordem_numerica(p.name)):
        texto = md.read_text(encoding="utf-8")
        lines = texto.splitlines()
        titulo = ""
        while lines and not titulo:
            lin = lines.pop(0)
            if lin.startswith("# "):
                titulo = lin[2:].strip()
        order += 1
        content = texto
        for lin in texto.splitlines():
            if lin.startswith("# "):
                content = texto.replace(lin, "", 1)
                break
        content = content.strip("\n")
        lessons.append(dict(id=order, title=titulo, content=content,
                            codeExample=None, codeLanguage=None, order=order))

    fase = dict(FASE_META[num_fase])
    fase["lessons"] = lessons
    fase["exercises"] = EXERCISES.get(num_fase, [])
    fase["challenge"] = None
    return fase


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", required=True, help="raiz do fundamentos-csharp")
    ap.add_argument("--fase", required=True, help="numero da fase (ex.: 0, 1 ou 'all')")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    repo_path = pathlib.Path(args.repo)
    out = pathlib.Path(args.out) if args.out else pathlib.Path(__file__).resolve().parents[1] / \
        "src/PortalEstudos.Infrastructure/Content/ContentSeed.json"

    fases_dict = {}
    if out.exists():
        try:
            dados = json.loads(out.read_text(encoding="utf-8"))
            for f in dados.get("fases", []):
                fases_dict[f["id"]] = f
        except Exception:
            pass

    if args.fase.lower() == "all":
        fases_alvo = sorted(FASE_META.keys())
    else:
        fases_alvo = [int(args.fase)]

    for f_id in fases_alvo:
        fase_data = carregar_fase(repo_path, f_id)
        fases_dict[f_id] = fase_data
        print(f"Fase {f_id}: {len(fase_data['lessons'])} licoes e {len(fase_data['exercises'])} exercicios espelhados.")

    fases_ordenadas = [fases_dict[k] for k in sorted(fases_dict.keys())]
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps({"fases": fases_ordenadas}, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Sucesso: {len(fases_ordenadas)} fases gravadas em {out}")


if __name__ == "__main__":
    main()
