using System.Globalization;
using Contas_App.Services;

namespace Contas_App.Pages;

/// <summary>
/// Quadro do mês corrente: o que há a pagar e a receber, ordenado por dia de vencimento,
/// com o total de cada lado e a diferença entre eles. Inclui as parcelas já pagas — o mês
/// só fecha quando se vê tudo o que passou por ele.
/// </summary>
public partial class ContasPage : ContentPage
{
    private static readonly CultureInfo Moeda = new("pt-BR");

    private readonly ResumoMensalService _resumoMensal;
    private readonly Color _corDiferenca;

    public ContasPage(ResumoMensalService resumoMensal)
    {
        InitializeComponent();
        _resumoMensal = resumoMensal;
        _corDiferenca = DiferencaLabel.TextColor;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async void OnRecarregarClicked(object? sender, EventArgs e) => await CarregarAsync();

    private async Task CarregarAsync()
    {
        EsconderStatus();
        SetBusy(true);
        var resumo = await _resumoMensal.ObterDoMesAsync(DateTime.Today);
        SetBusy(false);

        if (resumo is null)
        {
            MostrarStatus("Não foi possível carregar as contas do mês. Tente novamente.");
            return;
        }

        MesPagarLabel.Text = resumo.Mes;
        MesReceberLabel.Text = resumo.Mes;

        BindableLayout.SetItemsSource(PagarLista, resumo.Pagar);
        BindableLayout.SetItemsSource(ReceberLista, resumo.Receber);

        TotalPagarLabel.Text = Formatar(resumo.TotalPagar);
        TotalReceberLabel.Text = Formatar(resumo.TotalReceber);

        ResumoReceitasLabel.Text = Formatar(resumo.TotalReceber);
        ResumoDespesasLabel.Text = Formatar(resumo.TotalPagar);

        DiferencaLabel.Text = Formatar(resumo.Diferenca);
        DiferencaLabel.TextColor = resumo.Diferenca < 0 ? Colors.OrangeRed : _corDiferenca;
    }

    private static string Formatar(decimal valor) => valor.ToString("N2", Moeda);

    private void SetBusy(bool busy)
    {
        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
        RecarregarButton.IsEnabled = !busy;
    }

    private void MostrarStatus(string mensagem)
    {
        StatusLabel.Text = mensagem;
        StatusLabel.IsVisible = true;
    }

    private void EsconderStatus() => StatusLabel.IsVisible = false;
}
