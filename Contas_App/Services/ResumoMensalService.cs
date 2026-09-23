using System.Globalization;
using Contas_Contratos.Dto;

namespace Contas_App.Services;

/// <summary>
/// Monta o quadro do mês cruzando parcelas com dívidas: a parcela tem vencimento e valor,
/// mas é a dívida que diz o nome e se aquilo é a pagar ou a receber. Mesmo cruzamento do
/// QuadroAnualParcelas do Contas_Web — a API não expõe esse resumo pronto.
/// </summary>
public class ResumoMensalService(ParcelasApiService parcelasApi, DividasApiService dividasApi)
{
    /// <summary>Retorna null quando alguma das duas requisições falha.</summary>
    public async Task<ResumoMensal?> ObterDoMesAsync(DateTime referencia)
    {
        var parcelasTask = parcelasApi.ObterTodosAsync();
        var dividasTask = dividasApi.ObterTodosAsync();

        var parcelas = await parcelasTask;
        var dividas = await dividasTask;

        if (parcelas is null || dividas is null)
            return null;

        var porId = dividas.ToDictionary(d => d.Id);

        var doMes = parcelas
            .Where(p => p.Ativo
                && p.DataVencimento.Year == referencia.Year
                && p.DataVencimento.Month == referencia.Month)
            .OrderBy(p => p.DataVencimento)
            .Select(p => (Parcela: p, Divida: porId.GetValueOrDefault(p.IdDivida)))
            .ToList();

        return new ResumoMensal(referencia, Separar(doMes, ehDivida: true), Separar(doMes, ehDivida: false));
    }

    private static List<MovimentoItem> Separar(
        List<(ParcelaDto Parcela, DividaDto? Divida)> movimentos, bool ehDivida) =>
    [
        .. movimentos
            // Parcela órfã entra como a pagar, igual ao quadro anual do Contas_Web.
            .Where(m => (m.Divida?.EhDivida ?? true) == ehDivida)
            .Select(m => new MovimentoItem(
                Nome(m.Parcela, m.Divida),
                m.Parcela.DataVencimento.Day,
                m.Parcela.Valor,
                m.Parcela.Pago))
    ];

    private static string Nome(ParcelaDto parcela, DividaDto? divida)
    {
        if (!string.IsNullOrWhiteSpace(divida?.Nome))
            return divida.Nome;

        return string.IsNullOrWhiteSpace(parcela.Descricao) ? $"#{parcela.IdDivida}" : parcela.Descricao;
    }
}

/// <summary>Uma linha do quadro: a parcela de uma dívida ou receita que vence no mês.</summary>
public class MovimentoItem(string nome, int diaVencimento, decimal valor, bool pago)
{
    private static readonly CultureInfo Moeda = new("pt-BR");

    // O "✓" é o sinal do pago; a cor apagada sozinha seria ambígua numa lista longa.
    public string Nome { get; } = pago ? $"✓ {nome}" : nome;

    public string Vencimento { get; } = diaVencimento.ToString(Moeda);

    public decimal Valor { get; } = valor;

    public string ValorFormatado { get; } = valor.ToString("N2", Moeda);

    public Color Cor { get; } = pago ? Color.FromArgb("#9A9A9A") : Colors.White;
}

public class ResumoMensal(DateTime referencia, IReadOnlyList<MovimentoItem> pagar, IReadOnlyList<MovimentoItem> receber)
{
    private static readonly CultureInfo Moeda = new("pt-BR");

    /// <summary>Nome do mês com a inicial maiúscula, como no cabeçalho da planilha: Outubro.</summary>
    public string Mes { get; } = Capitalizar(Moeda.DateTimeFormat.GetMonthName(referencia.Month));

    public IReadOnlyList<MovimentoItem> Pagar => pagar;

    public IReadOnlyList<MovimentoItem> Receber => receber;

    public decimal TotalPagar { get; } = pagar.Sum(i => i.Valor);

    public decimal TotalReceber { get; } = receber.Sum(i => i.Valor);

    public decimal Diferenca => TotalReceber - TotalPagar;

    private static string Capitalizar(string texto) => char.ToUpper(texto[0], Moeda) + texto[1..];
}
