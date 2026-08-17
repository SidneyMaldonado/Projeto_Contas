using System.Globalization;

namespace Contas_Web;

/// <summary>Formatação pt-BR usada pelas listagens (moeda, datas e quantidades).</summary>
public static class Formato
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    public static string Data(DateTime data) => data.ToString("dd/MM/yyyy");

    public static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "—";

    public static string Quantidade(decimal valor) => valor.ToString("N4", PtBr);

    public static string Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? "—" : valor;
}
