using System.Globalization;

namespace Contas_Web;

/// <summary>Formatação pt-BR usada pelas listagens (moeda, datas e quantidades).</summary>
public static class Formato
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    /// <summary>Valor sem o símbolo da moeda, para tabelas com muitas colunas de dinheiro.</summary>
    public static string Valor(decimal valor) => valor.ToString("N2", PtBr);

    /// <summary>Nome abreviado do mês (1 a 12), com a inicial em maiúscula: Jan, Fev, Mar...</summary>
    public static string Mes(int mes)
    {
        var nome = PtBr.DateTimeFormat.GetAbbreviatedMonthName(mes);
        return char.ToUpper(nome[0], PtBr) + nome[1..];
    }

    public static string Data(DateTime data) => data.ToString("dd/MM/yyyy");

    public static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "—";

    public static string Quantidade(decimal valor) => valor.ToString("N4", PtBr);

    public static string Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? "—" : valor;
}
