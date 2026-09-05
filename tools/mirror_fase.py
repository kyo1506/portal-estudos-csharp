#!/usr/bin/env python3
"""
Espelha o conteúdo das Fases do repositorio 'fundamentos-csharp' para o
ContentSeed.json do portal (formato proprio do portal: licoes em Markdown +
exercicios interativos com solucao/esperado para o avaliador no browser).

Uso:
    python tools/mirror_fase.py --repo <caminho-fundamentos> --fase 0 \
        [--out src/PortalEstudos.Infrastructure/Content/ContentSeed.json]

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
}

FASE_META = {
    0: dict(id=0, title="Fundamentos da Programação (lógica com C#)",
            description="Do zero: variáveis e tipos, operadores, decisões, laços, arrays, strings, funções e lógica.",
            icon="🐣", challenge=None),
}


def ordem_numerica(name: str):
    m = re.match(r"(\d+)\.(\d+)", name)
    return (int(m.group(1)), int(m.group(2))) if m else (999, 0)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", required=True, help="raiz do fundamentos-csharp")
    ap.add_argument("--fase", type=int, required=True, help="numero da fase (ex.: 0)")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    fase_dir = [p for p in pathlib.Path(args.repo).glob("Fase-*") if p.is_dir() and f"-{args.fase:02d}" in p.name]
    if not fase_dir:
        sys.exit(f"pasta da Fase-{args.fase:02d} nao encontrada em {args.repo}")
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
        # remove a linha do H1 repetida no corpo (o portal mostra o título à parte)
        for lin in texto.splitlines():
            if lin.startswith("# "):
                content = texto.replace(lin, "", 1)
                break
        content = content.strip("\n")
        lessons.append(dict(id=order, title=titulo, content=content,
                            codeExample=None, codeLanguage=None, order=order))

    fase = dict(FASE_META[args.fase])
    fase["lessons"] = lessons
    fase["exercises"] = EXERCISES.get(args.fase, [])
    fase["challenge"] = None

    out = pathlib.Path(args.out) if args.out else pathlib.Path(__file__).resolve().parents[1] / \
        "src/PortalEstudos.Infrastructure/Content/ContentSeed.json"
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps({"fases": [fase]}, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"OK: {len(lessons)} licoes e {len(fase['exercises'])} exercicios -> {out}")


if __name__ == "__main__":
    main()
