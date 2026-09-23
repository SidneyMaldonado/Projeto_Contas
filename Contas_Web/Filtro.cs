using System.Globalization;
using System.Text;

namespace Contas_Web;

/// <summary>
/// Comparação usada pelo campo de busca das listagens: ignora maiúsculas/minúsculas
/// e acentuação, para "divida" encontrar "dívida" e "operacao" encontrar "operação".
/// </summary>
public static class Filtro
{
    public static bool Corresponde(string? termo, params string?[] campos)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return true;

        var alvo = Normalizar(termo);
        return campos.Any(campo => Normalizar(campo).Contains(alvo, StringComparison.Ordinal));
    }

    private static string Normalizar(string? texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(decomposto.Length);

        foreach (var caractere in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                resultado.Append(char.ToLowerInvariant(caractere));
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
