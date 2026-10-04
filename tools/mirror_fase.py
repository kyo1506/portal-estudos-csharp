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
    2: [
        dict(id=1, title="Encapsulamento de Conta com Validação", difficulty=0,
             description="Implemente a classe `Conta` com propriedade `decimal Saldo { get; private set; }` e métodos `void Depositar(decimal valor)` e `bool Sacar(decimal valor)`. `Depositar` lança `ArgumentOutOfRangeException` se valor <= 0. `Sacar` retorna `true` decrementando o saldo se houver fundos e valor > 0, senão `false`.",
             initialCode="using System;\n\n// TODO: implemente a classe Conta aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var c = new Conta(100m);\n        c.Depositar(50m);\n        var s1 = c.Sacar(30m);\n        var s2 = c.Sacar(200m);\n        Console.WriteLine($\"{c.Saldo} | {s1} | {s2}\");\n    }\n}",
             expectedOutput="120 | True | False",
             hints=["Use `public decimal Saldo { get; private set; }`.", "Valide se `valor <= 0` no depósito e saque."],
             solution="using System;\n\npublic class Conta\n{\n    public decimal Saldo { get; private set; }\n    public Conta(decimal saldoInicial = 0m)\n    {\n        Saldo = saldoInicial;\n    }\n    public void Depositar(decimal valor)\n    {\n        if (valor <= 0) throw new ArgumentOutOfRangeException(nameof(valor));\n        Saldo += valor;\n    }\n    public bool Sacar(decimal valor)\n    {\n        if (valor <= 0 || valor > Saldo) return false;\n        Saldo -= valor;\n        return true;\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var c = new Conta(100m);\n        c.Depositar(50m);\n        var s1 = c.Sacar(30m);\n        var s2 = c.Sacar(200m);\n        Console.WriteLine($\"{c.Saldo} | {s1} | {s2}\");\n    }\n}"),
        dict(id=2, title="Herança e Sobrescrita com Virtual e Override", difficulty=0,
             description="Crie a classe `Funcionario(decimal salario)` com método virtual `decimal CalcularBonus() => Salario * 0.10m;` e a subclasse `Gerente(decimal salario)` que sobrescreve para `(Salario * 0.20m) + 500m`.",
             initialCode="using System;\n\n// TODO: crie Funcionario e Gerente aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        Funcionario f1 = new Funcionario(3000m);\n        Funcionario f2 = new Gerente(5000m);\n        Console.WriteLine($\"{f1.CalcularBonus():F0} | {f2.CalcularBonus():F0}\");\n    }\n}",
             expectedOutput="300 | 1500",
             hints=["Use `virtual` no método da classe base.", "Use `override` na classe derivada."],
             solution="using System;\n\npublic class Funcionario\n{\n    public decimal Salario { get; }\n    public Funcionario(decimal salario) => Salario = salario;\n    public virtual decimal CalcularBonus() => Salario * 0.10m;\n}\n\npublic class Gerente : Funcionario\n{\n    public Gerente(decimal salario) : base(salario) {}\n    public override decimal CalcularBonus() => (Salario * 0.20m) + 500m;\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        Funcionario f1 = new Funcionario(3000m);\n        Funcionario f2 = new Gerente(5000m);\n        Console.WriteLine($\"{f1.CalcularBonus():F0} | {f2.CalcularBonus():F0}\");\n    }\n}"),
        dict(id=3, title="Interface de Notificação Polimórfica", difficulty=0,
             description="Crie a interface `INotificador` com `string Notificar(string destinatario, string mensagem)` e implemente as classes `EmailNotificador` e `SmsNotificador`.",
             initialCode="using System;\n\n// TODO: declare a interface INotificador e as classes aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        INotificador n1 = new EmailNotificador();\n        INotificador n2 = new SmsNotificador();\n        Console.WriteLine(n1.Notificar(\"ana@dev.com\", \"Bem-vinda\"));\n        Console.WriteLine(n2.Notificar(\"11999998888\", \"Codigo: 1234\"));\n    }\n}",
             expectedOutput="[EMAIL para ana@dev.com]: Bem-vinda\r\n[SMS para 11999998888]: Codigo: 1234",
             hints=["Defina `public interface INotificador { string Notificar(string destinatario, string mensagem); }`.", "Implemente a interface em ambas as classes."],
             solution="using System;\n\npublic interface INotificador\n{\n    string Notificar(string destinatario, string mensagem);\n}\n\npublic class EmailNotificador : INotificador\n{\n    public string Notificar(string destinatario, string mensagem) => $\"[EMAIL para {destinatario}]: {mensagem}\";\n}\n\npublic class SmsNotificador : INotificador\n{\n    public string Notificar(string destinatario, string mensagem) => $\"[SMS para {destinatario}]: {mensagem}\";\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        INotificador n1 = new EmailNotificador();\n        INotificador n2 = new SmsNotificador();\n        Console.WriteLine(n1.Notificar(\"ana@dev.com\", \"Bem-vinda\"));\n        Console.WriteLine(n2.Notificar(\"11999998888\", \"Codigo: 1234\"));\n    }\n}"),
        dict(id=4, title="Padrão Strategy de Descontos", difficulty=1,
             description="Crie `IDescontoStrategy` com `decimal Aplicar(decimal valor)`. Implemente `DescontoPadrao` (10% off) e `DescontoBlackFriday` (40% off), além de `ProcessadorPedido` que recebe a strategy no construtor.",
             initialCode="using System;\n\n// TODO: implemente IDescontoStrategy, as classes e ProcessadorPedido aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p1 = new ProcessadorPedido(new DescontoPadrao());\n        var p2 = new ProcessadorPedido(new DescontoBlackFriday());\n        Console.WriteLine($\"{p1.ObterTotal(100m):F0} | {p2.ObterTotal(100m):F0}\");\n    }\n}",
             expectedOutput="90 | 60",
             hints=["Na Strategy, multiplique por 0.90m e 0.60m.", "Injete `IDescontoStrategy` no construtor de `ProcessadorPedido`."],
             solution="using System;\n\npublic interface IDescontoStrategy\n{\n    decimal Aplicar(decimal valor);\n}\n\npublic class DescontoPadrao : IDescontoStrategy\n{\n    public decimal Aplicar(decimal valor) => valor * 0.90m;\n}\n\npublic class DescontoBlackFriday : IDescontoStrategy\n{\n    public decimal Aplicar(decimal valor) => valor * 0.60m;\n}\n\npublic class ProcessadorPedido\n{\n    private readonly IDescontoStrategy _strategy;\n    public ProcessadorPedido(IDescontoStrategy strategy) => _strategy = strategy;\n    public decimal ObterTotal(decimal valor) => _strategy.Aplicar(valor);\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p1 = new ProcessadorPedido(new DescontoPadrao());\n        var p2 = new ProcessadorPedido(new DescontoBlackFriday());\n        Console.WriteLine($\"{p1.ObterTotal(100m):F0} | {p2.ObterTotal(100m):F0}\");\n    }\n}"),
        dict(id=5, title="Validador Coeso de Usuário (SRP)", difficulty=0,
             description="Crie `Usuario(string Nome, string Email)` e a classe utilitária/validadora `UsuarioValidador` com `static bool EhValido(Usuario u)` (válido se nome >= 3 chars e email contiver '@').",
             initialCode="using System;\n\n// TODO: crie Usuario e UsuarioValidador aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var u1 = new Usuario(\"Bob\", \"bob@email.com\");\n        var u2 = new Usuario(\"Al\", \"al@email.com\");\n        var u3 = new Usuario(\"Carlos\", \"carlos-sem-arroba\");\n        Console.WriteLine($\"{UsuarioValidador.EhValido(u1)} | {UsuarioValidador.EhValido(u2)} | {UsuarioValidador.EhValido(u3)}\");\n    }\n}",
             expectedOutput="True | False | False",
             hints=["Separe a entidade de dados da lógica de validação.", "Verifique `u.Nome.Length >= 3` e `u.Email.Contains('@')`."],
             solution="using System;\n\npublic class Usuario\n{\n    public string Nome { get; }\n    public string Email { get;\n    }\n    public Usuario(string nome, string email)\n    {\n        Nome = nome;\n        Email = email;\n    }\n}\n\npublic static class UsuarioValidador\n{\n    public static bool EhValido(Usuario u)\n    {\n        if (u == null) return false;\n        return !string.IsNullOrWhiteSpace(u.Nome) && u.Nome.Length >= 3 && !string.IsNullOrWhiteSpace(u.Email) && u.Email.Contains('@');\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var u1 = new Usuario(\"Bob\", \"bob@email.com\");\n        var u2 = new Usuario(\"Al\", \"al@email.com\");\n        var u3 = new Usuario(\"Carlos\", \"carlos-sem-arroba\");\n        Console.WriteLine($\"{UsuarioValidador.EhValido(u1)} | {UsuarioValidador.EhValido(u2)} | {UsuarioValidador.EhValido(u3)}\");\n    }\n}"),
        dict(id=6, title="Inversão de Dependência com Repositório por Construtor", difficulty=1,
             description="Crie `IRepositorio<T>` com `void Adicionar(T item)` e `int Total()`. Crie `RepositorioMemoria<T>` e `ServicoCadastro` recebendo `IRepositorio<string>` no construtor.",
             initialCode="using System;\nusing System.Collections.Generic;\n\n// TODO: crie IRepositorio, RepositorioMemoria e ServicoCadastro aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        IRepositorio<string> repo = new RepositorioMemoria<string>();\n        var servico = new ServicoCadastro(repo);\n        servico.Cadastrar(\"CSharp\");\n        servico.Cadastrar(\"DotNet\");\n        Console.WriteLine(servico.ObterQuantidade());\n    }\n}",
             expectedOutput="2",
             hints=["Use `List<T>` internamente em `RepositorioMemoria<T>`.", "`ServicoCadastro` deve depender apenas de `IRepositorio<string>`."],
             solution="using System;\nusing System.Collections.Generic;\n\npublic interface IRepositorio<T>\n{\n    void Adicionar(T item);\n    int Total();\n}\n\npublic class RepositorioMemoria<T> : IRepositorio<T>\n{\n    private readonly List<T> _itens = new();\n    public void Adicionar(T item) => _itens.Add(item);\n    public int Total() => _itens.Count;\n}\n\npublic class ServicoCadastro\n{\n    private readonly IRepositorio<string> _repo;\n    public ServicoCadastro(IRepositorio<string> repo) => _repo = repo;\n    public void Cadastrar(string nome) => _repo.Adicionar(nome);\n    public int ObterQuantidade() => _repo.Total();\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        IRepositorio<string> repo = new RepositorioMemoria<string>();\n        var servico = new ServicoCadastro(repo);\n        servico.Cadastrar(\"CSharp\");\n        servico.Cadastrar(\"DotNet\");\n        Console.WriteLine(servico.ObterQuantidade());\n    }\n}"),
    ],
    3: [
        dict(id=1, title="Par Genérico Invertível", difficulty=0,
             description="Crie a classe genérica `Par<T1, T2>` com propriedades `Primeiro` e `Segundo` e método `void Inverter(out Par<T2, T1> invertido)`.",
             initialCode="using System;\n\n// TODO: implemente Par<T1, T2> aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p = new Par<string, int>(\"C#\", 10);\n        p.Inverter(out var inv);\n        Console.WriteLine($\"{p.Primeiro}:{p.Segundo} -> {inv.Primeiro}:{inv.Segundo}\");\n    }\n}",
             expectedOutput="C#:10 -> 10:C#",
             hints=["Defina `public class Par<T1, T2>(T1 primeiro, T2 segundo)`.", "O método deve atribuir `new Par<T2, T1>(Segundo, Primeiro)` ao parâmetro `out`."],
             solution="using System;\n\npublic class Par<T1, T2>\n{\n    public T1 Primeiro { get; }\n    public T2 Segundo { get; }\n    public Par(T1 primeiro, T2 segundo)\n    {\n        Primeiro = primeiro;\n        Segundo = segundo;\n    }\n    public void Inverter(out Par<T2, T1> invertido)\n    {\n        invertido = new Par<T2, T1>(Segundo, Primeiro);\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var p = new Par<string, int>(\"C#\", 10);\n        p.Inverter(out var inv);\n        Console.WriteLine($\"{p.Primeiro}:{p.Segundo} -> {inv.Primeiro}:{inv.Segundo}\");\n    }\n}"),
        dict(id=2, title="Filtro com Delegate Func", difficulty=0,
             description="Implemente o método genérico estático `Filtrar<T>(IEnumerable<T> fonte, Func<T, bool> predicado)` sem usar LINQ.",
             initialCode="using System;\nusing System.Collections.Generic;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var numeros = new[] { 1, 2, 3, 4, 5, 6 };\n        var pares = Filtrar(numeros, n => n % 2 == 0);\n        Console.WriteLine(string.Join(\", \", pares));\n    }\n\n    // TODO: implemente Filtrar aqui\n\n}",
             expectedOutput="2, 4, 6",
             hints=["Use um laço `foreach (var item in fonte)`.", "Se `predicado(item)` for true, adicione à lista resultante."],
             solution="using System;\nusing System.Collections.Generic;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var numeros = new[] { 1, 2, 3, 4, 5, 6 };\n        var pares = Filtrar(numeros, n => n % 2 == 0);\n        Console.WriteLine(string.Join(\", \", pares));\n    }\n\n    public static IEnumerable<T> Filtrar<T>(IEnumerable<T> fonte, Func<T, bool> predicado)\n    {\n        var res = new List<T>();\n        foreach (var item in fonte)\n            if (predicado(item)) res.Add(item);\n        return res;\n    }\n}"),
        dict(id=3, title="LINQ GroupBy: Contagem de Palavras", difficulty=1,
             description="Implemente `ContarPalavrasPorTamanho(string[] palavras)` usando LINQ para agrupar pelo tamanho da palavra e ordenar de forma crescente.",
             initialCode="using System;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var palavras = new[] { \"sol\", \"lua\", \"mar\", \"casa\", \"bola\", \"estrela\" };\n        foreach (var item in ContarPalavrasPorTamanho(palavras))\n            Console.WriteLine(item);\n    }\n\n    // TODO: implemente ContarPalavrasPorTamanho aqui\n\n}",
             expectedOutput="3: 3\r\n4: 2\r\n7: 1",
             hints=["Use `.GroupBy(p => p.Length).OrderBy(g => g.Key)`.", "Projete cada grupo como `$\"{g.Key}: {g.Count()}\"`."],
             solution="using System;\nusing System.Collections.Generic;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var palavras = new[] { \"sol\", \"lua\", \"mar\", \"casa\", \"bola\", \"estrela\" };\n        foreach (var item in ContarPalavrasPorTamanho(palavras))\n            Console.WriteLine(item);\n    }\n\n    static IEnumerable<string> ContarPalavrasPorTamanho(string[] palavras) =>\n        palavras\n            .GroupBy(p => p.Length)\n            .OrderBy(g => g.Key)\n            .Select(g => $\"{g.Key}: {g.Count()}\");\n}"),
        dict(id=4, title="LINQ Aggregate: Total com Desconto", difficulty=1,
             description="Implemente `CalcularTotalComDesconto(decimal[] itens, decimal taxaDesconto)` usando o operador LINQ `.Aggregate(...)`.",
             initialCode="using System;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var precos = new[] { 100m, 200m, 300m };\n        var total = CalcularTotalComDesconto(precos, 0.10m);\n        Console.WriteLine($\"{total:F0}\");\n    }\n\n    // TODO: implemente CalcularTotalComDesconto aqui\n\n}",
             expectedOutput="540",
             hints=["Use `.Aggregate(0m, (acc, item) => acc + (item * (1m - taxaDesconto)))`."],
             solution="using System;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var precos = new[] { 100m, 200m, 300m };\n        var total = CalcularTotalComDesconto(precos, 0.10m);\n        Console.WriteLine($\"{total:F0}\");\n    }\n\n    static decimal CalcularTotalComDesconto(decimal[] itens, decimal taxaDesconto) =>\n        itens.Aggregate(0m, (acc, item) => acc + (item * (1m - taxaDesconto)));\n}"),
        dict(id=5, title="LINQ Top N com OrderByDescending e Take", difficulty=0,
             description="Implemente `ObterTopN(int[] numeros, int n)` usando LINQ para retornar os `n` maiores números ordenados decrescente.",
             initialCode="using System;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var nums = new[] { 15, 3, 99, 42, 8, 77, 2 };\n        var top3 = ObterTopN(nums, 3);\n        Console.WriteLine(string.Join(\", \", top3));\n    }\n\n    // TODO: implemente ObterTopN aqui\n\n}",
             expectedOutput="99, 77, 42",
             hints=["Use `.OrderByDescending(x => x).Take(n)`."],
             solution="using System;\nusing System.Collections.Generic;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var nums = new[] { 15, 3, 99, 42, 8, 77, 2 };\n        var top3 = ObterTopN(nums, 3);\n        Console.WriteLine(string.Join(\", \", top3));\n    }\n\n    static IEnumerable<int> ObterTopN(int[] numeros, int n) =>\n        numeros.OrderByDescending(x => x).Take(n);\n}"),
        dict(id=6, title="Iterador Sob Demanda com yield return", difficulty=1,
             description="Implemente `GerarSequencia(int inicio, int passo, int quantidade)` usando `yield return`.",
             initialCode="using System;\nusing System.Collections.Generic;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var seq = GerarSequencia(10, 5, 4);\n        Console.WriteLine(string.Join(\" -> \", seq));\n    }\n\n    // TODO: implemente GerarSequencia aqui\n\n}",
             expectedOutput="10 -> 15 -> 20 -> 25",
             hints=["Use um laço `for (int i = 0; i < quantidade; i++)`.", "Emita cada termo com `yield return inicio + (i * passo);`."],
             solution="using System;\nusing System.Collections.Generic;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var seq = GerarSequencia(10, 5, 4);\n        Console.WriteLine(string.Join(\" -> \", seq));\n    }\n\n    static IEnumerable<int> GerarSequencia(int inicio, int passo, int quantidade)\n    {\n        for (int i = 0; i < quantidade; i++)\n            yield return inicio + (i * passo);\n    }\n}"),
    ],
    4: [
        dict(id=1, title="Padrão IDisposable com Flag de Descarte", difficulty=0,
             description="Implemente uma classe `RecursoTemporario : IDisposable` com a propriedade booleana `Disposed` e método `Dispose()`. Ao chamar `Dispose()`, se `Disposed` for false, defina `Disposed = true` e imprima \"Recurso liberado com sucesso!\". Chamadas subsequentes não devem imprimir nada.",
             initialCode="using System;\n\n// TODO: implemente a classe RecursoTemporario aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var rec = new RecursoTemporario();\n        rec.Dispose();\n        rec.Dispose(); // Segunda chamada não deve imprimir\n        Console.WriteLine($\"Disposed: {rec.Disposed}\");\n    }\n}",
             expectedOutput="Recurso liberado com sucesso!\r\nDisposed: True",
             hints=["Implemente `public bool Disposed { get; private set; }`.", "No `Dispose()`, teste `if (!Disposed)` antes de imprimir."],
             solution="using System;\n\npublic class RecursoTemporario : IDisposable\n{\n    public bool Disposed { get; private set; }\n\n    public void Dispose()\n    {\n        if (!Disposed)\n        {\n            Disposed = true;\n            Console.WriteLine(\"Recurso liberado com sucesso!\");\n        }\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var rec = new RecursoTemporario();\n        rec.Dispose();\n        rec.Dispose();\n        Console.WriteLine($\"Disposed: {rec.Disposed}\");\n    }\n}"),
        dict(id=2, title="Manipulação de Bytes com MemoryStream", difficulty=0,
             description="Implemente `ConverterParaBytes(string texto)` que grava o texto em um `MemoryStream` usando UTF-8 e retorna o array de bytes resultante.",
             initialCode="using System;\nusing System.IO;\nusing System.Text;\n\npublic class Program\n{\n    public static void Main()\n    {\n        byte[] dados = ConverterParaBytes(\"DotNet\");\n        Console.WriteLine($\"Bytes: {dados.Length} - {Encoding.UTF8.GetString(dados)}\");\n    }\n\n    // TODO: implemente ConverterParaBytes aqui\n\n}",
             expectedOutput="Bytes: 6 - DotNet",
             hints=["Crie `new MemoryStream()` dentro de um `using`.", "Escreva os bytes obtidos com `Encoding.UTF8.GetBytes(texto)`."],
             solution="using System;\nusing System.IO;\nusing System.Text;\n\npublic class Program\n{\n    public static void Main()\n    {\n        byte[] dados = ConverterParaBytes(\"DotNet\");\n        Console.WriteLine($\"Bytes: {dados.Length} - {Encoding.UTF8.GetString(dados)}\");\n    }\n\n    static byte[] ConverterParaBytes(string texto)\n    {\n        using var ms = new MemoryStream();\n        byte[] buffer = Encoding.UTF8.GetBytes(texto);\n        ms.Write(buffer, 0, buffer.Length);\n        return ms.ToArray();\n    }\n}"),
        dict(id=3, title="Serialização e Desserialização JSON", difficulty=0,
             description="Crie o record `Configuracao(string Servidor, int Porta)`. Serialize uma instância para string JSON e depois desserialize de volta, exibindo os dados no formato `Servidor:Porta`.",
             initialCode="using System;\nusing System.Text.Json;\n\n// TODO: declare o record Configuracao aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var original = new Configuracao(\"localhost\", 8080);\n        string json = JsonSerializer.Serialize(original);\n        var recuperado = JsonSerializer.Deserialize<Configuracao>(json);\n        Console.WriteLine($\"{recuperado.Servidor}:{recuperado.Porta}\");\n    }\n}",
             expectedOutput="localhost:8080",
             hints=["Declare `public record Configuracao(string Servidor, int Porta);`."],
             solution="using System;\nusing System.Text.Json;\n\npublic record Configuracao(string Servidor, int Porta);\n\npublic class Program\n{\n    public static void Main()\n    {\n        var original = new Configuracao(\"localhost\", 8080);\n        string json = JsonSerializer.Serialize(original);\n        var recuperado = JsonSerializer.Deserialize<Configuracao>(json);\n        Console.WriteLine($\"{recuperado.Servidor}:{recuperado.Porta}\");\n    }\n}"),
        dict(id=4, title="Composição Concorrente com Task.WhenAll", difficulty=1,
             description="Implemente o método assíncrono `SomarQuadradosAsync(int a, int b)` que calcula o quadrado de `a` e de `b` concorrentemente via `Task.Run` e retorna a soma de ambos aguardando com `Task.WhenAll`.",
             initialCode="using System;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        int resultado = await SomarQuadradosAsync(3, 4);\n        Console.WriteLine($\"Soma dos quadrados: {resultado}\");\n    }\n\n    // TODO: implemente SomarQuadradosAsync aqui\n\n}",
             expectedOutput="Soma dos quadrados: 25",
             hints=["Dispare `var t1 = Task.Run(() => a * a);` e `var t2 = Task.Run(() => b * b);`.", "Aguarde com `int[] res = await Task.WhenAll(t1, t2);`."],
             solution="using System;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        int resultado = await SomarQuadradosAsync(3, 4);\n        Console.WriteLine($\"Soma dos quadrados: {resultado}\");\n    }\n\n    static async Task<int> SomarQuadradosAsync(int a, int b)\n    {\n        var t1 = Task.Run(() => a * a);\n        var t2 = Task.Run(() => b * b);\n        int[] resultados = await Task.WhenAll(t1, t2);\n        return resultados[0] + resultados[1];\n    }\n}"),
        dict(id=5, title="Cancelamento Cooperativo com CancellationToken", difficulty=1,
             description="Implemente `ContarComCancelamentoAsync(int limite, CancellationToken token)` que itera de 1 a limite. A cada passo, chame `token.ThrowIfCancellationRequested()`. No Main, configure um token já cancelado, capture `OperationCanceledException` e imprima \"Cancelamento interceptado com sucesso!\".",
             initialCode="using System;\nusing System.Threading;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        using var cts = new CancellationTokenSource();\n        cts.Cancel(); // Cancela antes de iniciar\n\n        try\n        {\n            await ContarComCancelamentoAsync(10, cts.Token);\n        }\n        catch (OperationCanceledException)\n        {\n            Console.WriteLine(\"Cancelamento interceptado com sucesso!\");\n        }\n    }\n\n    // TODO: implemente ContarComCancelamentoAsync aqui\n\n}",
             expectedOutput="Cancelamento interceptado com sucesso!",
             hints=["Use `token.ThrowIfCancellationRequested();` no início do método ou dentro do laço."],
             solution="using System;\nusing System.Threading;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        using var cts = new CancellationTokenSource();\n        cts.Cancel();\n\n        try\n        {\n            await ContarComCancelamentoAsync(10, cts.Token);\n        }\n        catch (OperationCanceledException)\n        {\n            Console.WriteLine(\"Cancelamento interceptado com sucesso!\");\n        }\n    }\n\n    static Task ContarComCancelamentoAsync(int limite, CancellationToken token)\n    {\n        token.ThrowIfCancellationRequested();\n        return Task.CompletedTask;\n    }\n}"),
        dict(id=6, title="Listar Propriedades com Reflection", difficulty=1,
             description="Implemente a função genérica `ObterNomesPropriedades<T>()` que retorna os nomes de todas as propriedades públicas de instância de `T` ordenadas alfabeticamente.",
             initialCode="using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Reflection;\n\npublic record Aluno(int Id, string Nome, string Email);\n\npublic class Program\n{\n    public static void Main()\n    {\n        var nomes = ObterNomesPropriedades<Aluno>();\n        Console.WriteLine(string.Join(\", \", nomes));\n    }\n\n    // TODO: implemente ObterNomesPropriedades aqui\n\n}",
             expectedOutput="Email, Id, Nome",
             hints=["Use `typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)`.", "Ordene com `.Select(p => p.Name).OrderBy(n => n)`."],
             solution="using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Reflection;\n\npublic record Aluno(int Id, string Nome, string Email);\n\npublic class Program\n{\n    public static void Main()\n    {\n        var nomes = ObterNomesPropriedades<Aluno>();\n        Console.WriteLine(string.Join(\", \", nomes));\n    }\n\n    static IEnumerable<string> ObterNomesPropriedades<T>() =>\n        typeof(T)\n            .GetProperties(BindingFlags.Public | BindingFlags.Instance)\n            .Select(p => p.Name)\n            .OrderBy(n => n);\n}"),
    ],
    5: [
        dict(id=1, title="Padrão Strategy de Frete", difficulty=0,
             description="Crie a interface `IPoliticaFrete` com `decimal Calcular(decimal pesoKg)` e as implementações `FreteExpresso` (R$ 15 por kg) e `FreteEconomico` (R$ 5 por kg).",
             initialCode="using System;\n\n// TODO: crie IPoliticaFrete, FreteExpresso e FreteEconomico aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        IPoliticaFrete exp = new FreteExpresso();\n        IPoliticaFrete eco = new FreteEconomico();\n        Console.WriteLine($\"Expresso: {exp.Calcular(10):F2}\");\n        Console.WriteLine($\"Economico: {eco.Calcular(10):F2}\");\n    }\n}",
             expectedOutput="Expresso: 150,00\r\nEconomico: 50,00",
             hints=["Multiplique o peso pelo valor unitário correspondente."],
             solution="using System;\n\npublic interface IPoliticaFrete\n{\n    decimal Calcular(decimal pesoKg);\n}\n\npublic class FreteExpresso : IPoliticaFrete\n{\n    public decimal Calcular(decimal pesoKg) => pesoKg * 15m;\n}\n\npublic class FreteEconomico : IPoliticaFrete\n{\n    public decimal Calcular(decimal pesoKg) => pesoKg * 5m;\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        IPoliticaFrete exp = new FreteExpresso();\n        IPoliticaFrete eco = new FreteEconomico();\n        Console.WriteLine($\"Expresso: {exp.Calcular(10):F2}\");\n        Console.WriteLine($\"Economico: {eco.Calcular(10):F2}\");\n    }\n}"),
        dict(id=2, title="Padrão Decorator com Notificador", difficulty=0,
             description="Crie a interface `INotificador` com `string Notificar(string msg)`. Crie a classe base `NotificadorBase` que retorna `msg`. Crie o decorador `NotificadorUrgenteDecorator` que anexa \"[URGENTE] \" no início da mensagem retornada pelo inner notificador.",
             initialCode="using System;\n\n// TODO: implemente INotificador, NotificadorBase e NotificadorUrgenteDecorator aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        INotificador baseNotif = new NotificadorBase();\n        INotificador decorado = new NotificadorUrgenteDecorator(baseNotif);\n        Console.WriteLine(decorado.Notificar(\"Servidor reiniciando!\"));\n    }\n}",
             expectedOutput="[URGENTE] Servidor reiniciando!",
             hints=["O decorador deve receber `INotificador inner` no construtor.", "No método Notificar, retorne `$\"[URGENTE] {_inner.Notificar(msg)}\"`."],
             solution="using System;\n\npublic interface INotificador\n{\n    string Notificar(string msg);\n}\n\npublic class NotificadorBase : INotificador\n{\n    public string Notificar(string msg) => msg;\n}\n\npublic class NotificadorUrgenteDecorator : INotificador\n{\n    private readonly INotificador _inner;\n    public NotificadorUrgenteDecorator(INotificador inner) => _inner = inner;\n    public string Notificar(string msg) => $\"[URGENTE] {_inner.Notificar(msg)}\";\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        INotificador baseNotif = new NotificadorBase();\n        INotificador decorado = new NotificadorUrgenteDecorator(baseNotif);\n        Console.WriteLine(decorado.Notificar(\"Servidor reiniciando!\"));\n    }\n}"),
        dict(id=3, title="Injeção de Dependência Básica", difficulty=0,
             description="Crie a interface `ISaudacao` com método `string Cumprimentar(string nome)` e a implementação `SaudacaoFormal` retornando \"Prezado(a) [nome]\". No Main, use uma instância de `SaudacaoFormal` diretamente ou simule o container resolvendo a interface.",
             initialCode="using System;\n\n// TODO: crie ISaudacao e SaudacaoFormal aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        ISaudacao servico = new SaudacaoFormal();\n        Console.WriteLine(servico.Cumprimentar(\"Lucas\"));\n    }\n}",
             expectedOutput="Prezado(a) Lucas",
             hints=["Implemente `public string Cumprimentar(string nome) => $\"Prezado(a) {nome}\";`."],
             solution="using System;\n\npublic interface ISaudacao\n{\n    string Cumprimentar(string nome);\n}\n\npublic class SaudacaoFormal : ISaudacao\n{\n    public string Cumprimentar(string nome) => $\"Prezado(a) {nome}\";\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        ISaudacao servico = new SaudacaoFormal();\n        Console.WriteLine(servico.Cumprimentar(\"Lucas\"));\n    }\n}"),
        dict(id=4, title="Repositório Fake em Memória", difficulty=1,
             description="Crie a interface `IRepositorio<T>` com `void Adicionar(T item)` e `int Total()`. Crie a classe `RepositorioEmMemoria<T>` utilizando uma `List<T>` interna.",
             initialCode="using System;\nusing System.Collections.Generic;\n\n// TODO: crie IRepositorio<T> e RepositorioEmMemoria<T> aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var repo = new RepositorioEmMemoria<string>();\n        repo.Adicionar(\"Item A\");\n        repo.Adicionar(\"Item B\");\n        Console.WriteLine($\"Total: {repo.Total()}\");\n    }\n}",
             expectedOutput="Total: 2",
             hints=["Encapsule uma `new List<T>()` como campo privado."],
             solution="using System;\nusing System.Collections.Generic;\n\npublic interface IRepositorio<T>\n{\n    void Adicionar(T item);\n    int Total();\n}\n\npublic class RepositorioEmMemoria<T> : IRepositorio<T>\n{\n    private readonly List<T> _itens = new();\n    public void Adicionar(T item) => _itens.Add(item);\n    public int Total() => _itens.Count;\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var repo = new RepositorioEmMemoria<string>();\n        repo.Adicionar(\"Item A\");\n        repo.Adicionar(\"Item B\");\n        Console.WriteLine($\"Total: {repo.Total()}\");\n    }\n}"),
        dict(id=5, title="Validação com Exceção de Domínio", difficulty=1,
             description="Crie a classe `DomainException : Exception`. Crie o método estático `ValidarIdade(int idade)` que lança `DomainException(\"Idade mínima é 18 anos.\")` se idade < 18. No Main capture e imprima a mensagem.",
             initialCode="using System;\n\n// TODO: declare DomainException e ValidarIdade aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        try\n        {\n            ValidarIdade(16);\n        }\n        catch (DomainException ex)\n        {\n            Console.WriteLine($\"Erro: {ex.Message}\");\n        }\n    }\n}",
             expectedOutput="Erro: Idade mínima é 18 anos.",
             hints=["Faça `public class DomainException : Exception { public DomainException(string msg) : base(msg) {} }`."],
             solution="using System;\n\npublic class DomainException : Exception\n{\n    public DomainException(string msg) : base(msg) { }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        try\n        {\n            ValidarIdade(16);\n        }\n        catch (DomainException ex)\n        {\n            Console.WriteLine($\"Erro: {ex.Message}\");\n        }\n    }\n\n    static void ValidarIdade(int idade)\n    {\n        if (idade < 18)\n            throw new DomainException(\"Idade mínima é 18 anos.\");\n    }\n}"),
        dict(id=6, title="Tabela de Decisão para Classificação", difficulty=1,
             description="Implemente `AvaliarDesempenho(decimal nota, int presenca)` retornando \"Excelente\" se nota >= 9 e presenca >= 90; \"Aprovado\" se nota >= 6 e presenca >= 75; senão \"Reprovado\".",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(AvaliarDesempenho(9.5m, 95));\n        Console.WriteLine(AvaliarDesempenho(7.0m, 80));\n        Console.WriteLine(AvaliarDesempenho(9.0m, 60));\n    }\n\n    // TODO: implemente AvaliarDesempenho aqui\n\n}",
             expectedOutput="Excelente\r\nAprovado\r\nReprovado",
             hints=["Use `if (nota >= 9m && presenca >= 90)` primeiro."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(AvaliarDesempenho(9.5m, 95));\n        Console.WriteLine(AvaliarDesempenho(7.0m, 80));\n        Console.WriteLine(AvaliarDesempenho(9.0m, 60));\n    }\n\n    static string AvaliarDesempenho(decimal nota, int presenca)\n    {\n        if (nota >= 9m && presenca >= 90) return \"Excelente\";\n        if (nota >= 6m && presenca >= 75) return \"Aprovado\";\n        return \"Reprovado\";\n    }\n}"),
    ],
    6: [
        dict(id=1, title="Endpoint com TypedResults", difficulty=0,
             description="Crie o método `SimularEndpointCurso(int id, bool existe)` que retorna \"200 OK: Curso Encontrado\" se existe for true, e \"404 Not Found\" caso contrário.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SimularEndpointCurso(1, true));\n        Console.WriteLine(SimularEndpointCurso(2, false));\n    }\n\n    // TODO: implemente SimularEndpointCurso aqui\n\n}",
             expectedOutput="200 OK: Curso Encontrado\r\n404 Not Found",
             hints=["Use um `if (existe)` ou operador ternário."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SimularEndpointCurso(1, true));\n        Console.WriteLine(SimularEndpointCurso(2, false));\n    }\n\n    static string SimularEndpointCurso(int id, bool existe) =>\n        existe ? \"200 OK: Curso Encontrado\" : \"404 Not Found\";\n}"),
        dict(id=2, title="Validação de Payload de Curso", difficulty=0,
             description="Implemente `ValidarCurso(string titulo, decimal preco, int vagas)` retornando `true` se titulo tiver pelo menos 3 caracteres, preco > 0 e vagas > 0, senão `false`.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ValidarCurso(\"API .NET\", 500m, 10));\n        Console.WriteLine(ValidarCurso(\"C#\", 500m, 10));\n        Console.WriteLine(ValidarCurso(\"Java\", -10m, 5));\n    }\n\n    // TODO: implemente ValidarCurso aqui\n\n}",
             expectedOutput="True\r\nFalse\r\nFalse",
             hints=["Teste `!string.IsNullOrWhiteSpace(titulo) && titulo.Length >= 3`."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ValidarCurso(\"API .NET\", 500m, 10));\n        Console.WriteLine(ValidarCurso(\"C#\", 500m, 10));\n        Console.WriteLine(ValidarCurso(\"Java\", -10m, 5));\n    }\n\n    static bool ValidarCurso(string titulo, decimal preco, int vagas) =>\n        !string.IsNullOrWhiteSpace(titulo) && titulo.Trim().Length >= 3 && preco > 0 && vagas > 0;\n}"),
        dict(id=3, title="Estrutura ProblemDetails RFC 7807", difficulty=1,
             description="Crie a classe `ProblemDetails(string Title, int Status, string Detail)`. Implemente `CriarErroValidacao(string detalhe)` retornando um ProblemDetails com Title=\"Erro de Validação\" e Status=400.",
             initialCode="using System;\n\n// TODO: declare ProblemDetails e CriarErroValidacao aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var erro = CriarErroValidacao(\"Preço inválido.\");\n        Console.WriteLine($\"{erro.Status}: {erro.Title} - {erro.Detail}\");\n    }\n}",
             expectedOutput="400: Erro de Validação - Preço inválido.",
             hints=["Declare `public record ProblemDetails(string Title, int Status, string Detail);`."],
             solution="using System;\n\npublic record ProblemDetails(string Title, int Status, string Detail);\n\npublic class Program\n{\n    public static void Main()\n    {\n        var erro = CriarErroValidacao(\"Preço inválido.\");\n        Console.WriteLine($\"{erro.Status}: {erro.Title} - {erro.Detail}\");\n    }\n\n    static ProblemDetails CriarErroValidacao(string detalhe) =>\n        new ProblemDetails(\"Erro de Validação\", 400, detalhe);\n}"),
        dict(id=4, title="Filtro LINQ de Cursos com Vagas", difficulty=0,
             description="Implemente `ObterCursosDisponiveis(IEnumerable<(string Titulo, int Ocupadas, int Total)> cursos)` retornando apenas títulos com vagas disponíveis (Ocupadas < Total) ordenados alfabeticamente.",
             initialCode="using System;\nusing System.Collections.Generic;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var lista = new[]\n        {\n            (\"C#\", 10, 10),\n            (\"ASP.NET\", 3, 5),\n            (\"Docker\", 1, 4)\n        };\n        var disp = ObterCursosDisponiveis(lista);\n        Console.WriteLine(string.Join(\", \", disp));\n    }\n\n    // TODO: implemente ObterCursosDisponiveis aqui\n\n}",
             expectedOutput="ASP.NET, Docker",
             hints=["Use `.Where(c => c.Ocupadas < c.Total).OrderBy(c => c.Titulo).Select(c => c.Titulo)`."],
             solution="using System;\nusing System.Collections.Generic;\nusing System.Linq;\n\npublic class Program\n{\n    public static void Main()\n    {\n        var lista = new[]\n        {\n            (\"C#\", 10, 10),\n            (\"ASP.NET\", 3, 5),\n            (\"Docker\", 1, 4)\n        };\n        var disp = ObterCursosDisponiveis(lista);\n        Console.WriteLine(string.Join(\", \", disp));\n    }\n\n    static IEnumerable<string> ObterCursosDisponiveis(IEnumerable<(string Titulo, int Ocupadas, int Total)> cursos) =>\n        cursos.Where(c => c.Ocupadas < c.Total).OrderBy(c => c.Titulo).Select(c => c.Titulo);\n}"),
        dict(id=5, title="Simulação de Claims JWT", difficulty=1,
             description="Implemente `CriarPayloadClaims(string sub, string role)` retornando `$\"sub={sub};role={role}\"`. No Main, chame com sub=\"user123\" e role=\"Admin\".",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(CriarPayloadClaims(\"user123\", \"Admin\"));\n    }\n\n    // TODO: implemente CriarPayloadClaims aqui\n\n}",
             expectedOutput="sub=user123;role=Admin",
             hints=["Faça interpolação de strings simples com `$`: `$\"sub={sub};role={role}\"`."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(CriarPayloadClaims(\"user123\", \"Admin\"));\n    }\n\n    static string CriarPayloadClaims(string sub, string role) =>\n        $\"sub={sub};role={role}\";\n}"),
        dict(id=6, title="Health Check de Liveness e Readiness", difficulty=0,
             description="Implemente `AvaliarSaude(bool dbOk, bool cacheOk)` retornando \"Healthy\" se ambos forem true, \"Degraded\" se apenas um for true, senão \"Unhealthy\".",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(AvaliarSaude(true, true));\n        Console.WriteLine(AvaliarSaude(true, false));\n        Console.WriteLine(AvaliarSaude(false, false));\n    }\n\n    // TODO: implemente AvaliarSaude aqui\n\n}",
             expectedOutput="Healthy\r\nDegraded\r\nUnhealthy",
             hints=["Use pattern matching com tupla: `(dbOk, cacheOk) switch { ... }`."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(AvaliarSaude(true, true));\n        Console.WriteLine(AvaliarSaude(true, false));\n        Console.WriteLine(AvaliarSaude(false, false));\n    }\n\n    static string AvaliarSaude(bool dbOk, bool cacheOk) => (dbOk, cacheOk) switch\n    {\n        (true, true) => \"Healthy\",\n        (true, false) or (false, true) => \"Degraded\",\n        _ => \"Unhealthy\"\n    };\n}"),
    ],
    7: [
        dict(id=1, title="Span e Slicing de Texto Zero-Allocation", difficulty=0,
             description="Implemente `ExtrairDominioEmail(string email)` utilizando `email.AsSpan()` e `.IndexOf('@')` para retornar o domínio do e-mail sem alocações adicionais no Heap.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ExtrairDominioEmail(\"dev@dotnet.microsoft.com\"));\n        Console.WriteLine(ExtrairDominioEmail(\"aluno@faculdade.edu.br\"));\n    }\n\n    // TODO: implemente ExtrairDominioEmail aqui\n\n}",
             expectedOutput="dotnet.microsoft.com\r\nfaculdade.edu.br",
             hints=["Use email.AsSpan().", "Localize o '@' com span.IndexOf('@') e faça o Slice."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ExtrairDominioEmail(\"dev@dotnet.microsoft.com\"));\n        Console.WriteLine(ExtrairDominioEmail(\"aluno@faculdade.edu.br\"));\n    }\n\n    static string ExtrairDominioEmail(string email)\n    {\n        var span = email.AsSpan();\n        int idx = span.IndexOf('@');\n        return idx >= 0 ? span.Slice(idx + 1).ToString() : string.Empty;\n    }\n}"),
        dict(id=2, title="Parsing de Inteiro sem Substring", difficulty=1,
             description="Implemente `SomarValoresPipe(string registro)` que recebe uma string como \"10|25|15\" e soma os valores utilizando `ReadOnlySpan<char>` e `int.Parse(span)` sem chamar `string.Split()`.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SomarValoresPipe(\"10|25|15\"));\n        Console.WriteLine(SomarValoresPipe(\"100|200|300|400\"));\n    }\n\n    // TODO: implemente SomarValoresPipe aqui\n\n}",
             expectedOutput="50\r\n1000",
             hints=["Percorra a string com span.IndexOf('|') iterativamente.", "Use int.Parse(span.Slice(...)) em cada parte."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(SomarValoresPipe(\"10|25|15\"));\n        Console.WriteLine(SomarValoresPipe(\"100|200|300|400\"));\n    }\n\n    static int SomarValoresPipe(string registro)\n    {\n        var span = registro.AsSpan();\n        int soma = 0;\n        while (!span.IsEmpty)\n        {\n            int idx = span.IndexOf('|');\n            if (idx == -1)\n            {\n                soma += int.Parse(span);\n                break;\n            }\n            soma += int.Parse(span.Slice(0, idx));\n            span = span.Slice(idx + 1);\n        }\n        return soma;\n    }\n}"),
        dict(id=3, title="Validação de Código de Transação", difficulty=0,
             description="Implemente `ValidarCodigoTransacao(string codigo)` que valida se o código obedece ao formato \"TX-\" seguido de 4 caracteres alfanuméricos maiúsculos (ex: \"TX-A1B2\").",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ValidarCodigoTransacao(\"TX-A1B2\"));\n        Console.WriteLine(ValidarCodigoTransacao(\"tx-a1b2\"));\n        Console.WriteLine(ValidarCodigoTransacao(\"TX-123\"));\n    }\n\n    // TODO: implemente ValidarCodigoTransacao aqui\n\n}",
             expectedOutput="True\r\nFalse\r\nFalse",
             hints=["Verifique se o tamanho é exatamente 7.", "Valide se começa com 'TX-' e os 4 seguintes são alfanuméricos maiúsculos."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(ValidarCodigoTransacao(\"TX-A1B2\"));\n        Console.WriteLine(ValidarCodigoTransacao(\"tx-a1b2\"));\n        Console.WriteLine(ValidarCodigoTransacao(\"TX-123\"));\n    }\n\n    static bool ValidarCodigoTransacao(string codigo)\n    {\n        if (codigo == null || codigo.Length != 7 || !codigo.StartsWith(\"TX-\")) return false;\n        for (int i = 3; i < 7; i++)\n        {\n            char c = codigo[i];\n            if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))) return false;\n        }\n        return true;\n    }\n}"),
        dict(id=4, title="Fila Concorrente com Channel", difficulty=1,
             description="Simule um produtor e consumidor assíncronos com `Channel.CreateUnbounded<int>()`. Implemente `CalcularTotalAsync(int quantidade)` que escreve de 1 até `quantidade`, completa o escritor e retorna a soma lida do leitor.",
             initialCode="using System;\nusing System.Threading.Channels;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        Console.WriteLine(await CalcularTotalAsync(5));\n        Console.WriteLine(await CalcularTotalAsync(10));\n    }\n\n    // TODO: implemente CalcularTotalAsync aqui\n\n}",
             expectedOutput="15\r\n55",
             hints=["Crie var canal = Channel.CreateUnbounded<int>();", "Escreva no canal em loop e chame canal.Writer.Complete().", "Leia com await foreach (var item in canal.Reader.ReadAllAsync()) e some."],
             solution="using System;\nusing System.Threading.Channels;\nusing System.Threading.Tasks;\n\npublic class Program\n{\n    public static async Task Main()\n    {\n        Console.WriteLine(await CalcularTotalAsync(5));\n        Console.WriteLine(await CalcularTotalAsync(10));\n    }\n\n    static async Task<int> CalcularTotalAsync(int quantidade)\n    {\n        var canal = Channel.CreateUnbounded<int>();\n        for (int i = 1; i <= quantidade; i++)\n        {\n            canal.Writer.TryWrite(i);\n        }\n        canal.Writer.Complete();\n\n        int soma = 0;\n        await foreach (var item in canal.Reader.ReadAllAsync())\n        {\n            soma += item;\n        }\n        return soma;\n    }\n}"),
        dict(id=5, title="Ref Struct de Leitura de Protocolo", difficulty=1,
             description="Implemente o `ref struct LeitorComando` recebendo `ReadOnlySpan<char>` e o método `bool TryLerComando(out ReadOnlySpan<char> comando, out ReadOnlySpan<char> argumento)` separando por ':'.",
             initialCode="using System;\n\n// TODO: implemente o ref struct LeitorComando aqui\n\npublic class Program\n{\n    public static void Main()\n    {\n        var leitor = new LeitorComando(\"SET:TAXA_JUROS_12\".AsSpan());\n        if (leitor.TryLerComando(out var cmd, out var arg))\n        {\n            Console.WriteLine($\"{cmd.ToString()} => {arg.ToString()}\");\n        }\n    }\n}",
             expectedOutput="SET => TAXA_JUROS_12",
             hints=["Declare public readonly ref struct LeitorComando(ReadOnlySpan<char> buffer).", "Use buffer.IndexOf(':') para separar comando e argumento."],
             solution="using System;\n\npublic readonly ref struct LeitorComando\n{\n    private readonly ReadOnlySpan<char> _buffer;\n    public LeitorComando(ReadOnlySpan<char> buffer) => _buffer = buffer;\n\n    public bool TryLerComando(out ReadOnlySpan<char> comando, out ReadOnlySpan<char> argumento)\n    {\n        int idx = _buffer.IndexOf(':');\n        if (idx == -1)\n        {\n            comando = default;\n            argumento = default;\n            return false;\n        }\n        comando = _buffer.Slice(0, idx);\n        argumento = _buffer.Slice(idx + 1);\n        return true;\n    }\n}\n\npublic class Program\n{\n    public static void Main()\n    {\n        var leitor = new LeitorComando(\"SET:TAXA_JUROS_12\".AsSpan());\n        if (leitor.TryLerComando(out var cmd, out var arg))\n        {\n            Console.WriteLine($\"{cmd.ToString()} => {arg.ToString()}\");\n        }\n    }\n}"),
        dict(id=6, title="Média Ponderada com ReadOnlySpan", difficulty=0,
             description="Implemente `CalcularMediaPonderada(ReadOnlySpan<double> valores, ReadOnlySpan<double> pesos)` que calcula a média ponderada (soma(v * p) / soma(p)) iterando com spans.",
             initialCode="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        double[] v = [8.0, 6.0, 10.0];\n        double[] p = [2.0, 3.0, 5.0];\n        Console.WriteLine(CalcularMediaPonderada(v, p).ToString(\"F2\", System.Globalization.CultureInfo.InvariantCulture));\n    }\n\n    // TODO: implemente CalcularMediaPonderada aqui\n\n}",
             expectedOutput="8.40",
             hints=["Itere até valores.Length acumulando a soma dos produtos e a soma dos pesos.", "Divida os dois acumuladores."],
             solution="using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        double[] v = [8.0, 6.0, 10.0];\n        double[] p = [2.0, 3.0, 5.0];\n        Console.WriteLine(CalcularMediaPonderada(v, p).ToString(\"F2\", System.Globalization.CultureInfo.InvariantCulture));\n    }\n\n    static double CalcularMediaPonderada(ReadOnlySpan<double> valores, ReadOnlySpan<double> pesos)\n    {\n        if (valores.Length != pesos.Length || valores.IsEmpty) return 0;\n        double somaProdutos = 0;\n        double somaPesos = 0;\n        for (int i = 0; i < valores.Length; i++)\n        {\n            somaProdutos += valores[i] * pesos[i];\n            somaPesos += pesos[i];\n        }\n        return somaPesos > 0 ? somaProdutos / somaPesos : 0;\n    }\n}"),
    ],
}

FASE_META = {
    0: dict(id=0, title="Fundamentos da Programação (lógica com C#)",
            description="Do zero: variáveis e tipos, operadores, decisões, laços, arrays, strings, funções e lógica.",
            icon="🐣", challenge=None),
    1: dict(id=1, title="C# Básico: a linguagem em profundidade",
            description="Tipos por valor vs referência, strings/StringBuilder, decimal, coleções, exceções, structs/records e pattern matching.",
            icon="⚙️", challenge=None),
    2: dict(id=2, title="Programação Orientada a Objetos + SOLID",
            description="Classes, objetos, encapsulamento, herança, polimorfismo, interfaces, composição vs herança, SOLID e DI por construtor.",
            icon="🏛️", challenge=None),
    3: dict(id=3, title="Coleções avançadas, Delegados, Eventos, Generics e LINQ",
            description="Tipos genéricos, delegates/Func/Action, lambdas, eventos com EventHandler, LINQ fluente, GroupBy, Aggregate e yield return.",
            icon="⚡", challenge=None),
    4: dict(id=4, title=".NET Runtime & BCL (IO, JSON, Threads e Async)",
            description="Garbage Collector/IDisposable, Streams/Buffers, System.Text.Json e Source Generators, async/await, CancellationToken, TimeProvider e Reflection.",
            icon="🔄", challenge=None),
    5: dict(id=5, title="Arquitetura, Padrões, DI e Testes (TDD)",
            description="Clean Architecture, Strategy/Factory/Repository/Decorator, Microsoft.Extensions.DependencyInjection, xUnit (Facts/Theories) e Fakes/Mocks.",
            icon="🏗️", challenge=None),
    6: dict(id=6, title="Aplicações Web e Serviços (.NET Minimal APIs)",
            description="RESTful HTTP, ASP.NET Core Minimal APIs, Middlewares, ProblemDetails (RFC 7807), EF Core, Autenticação JWT e Docker.",
            icon="🌐", challenge=None),
    7: dict(id=7, title="Tópicos Avançados & Performance",
            description="Engenharia de Performance, Span/Memory, ref struct, System.Threading.Channels, Source Generators e BenchmarkDotNet.",
            icon="🚀", challenge=None),
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
