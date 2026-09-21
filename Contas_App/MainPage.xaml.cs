using System.Collections.ObjectModel;
using System.Globalization;
using Contas_App.Services;

namespace Contas_App;

public partial class MainPage : ContentPage
{
    private static readonly CultureInfo Moeda = new("pt-BR");

    private readonly ContasApiService _contasApi;
    private readonly ObservableCollection<ContaSaldoItem> _contas = [];

    public MainPage(ContasApiService contasApi)
    {
        InitializeComponent();
        _contasApi = contasApi;
        ContasCollection.ItemsSource = _contas;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContasAsync();
    }

    private async Task CarregarContasAsync()
    {
        EsconderStatus();
        SetBusy(true);
        var resumo = await _contasApi.ObterResumoAsync();
        SetBusy(false);

        if (resumo is null)
        {
            MostrarStatus("Não foi possível carregar as contas. Tente novamente.", erro: true);
            return;
        }

        _contas.Clear();
        foreach (var conta in resumo)
            _contas.Add(new ContaSaldoItem(conta));

        AtualizarTotal();
        SairDaEdicao();
    }

    private async void OnAtualizarSaldosClicked(object? sender, EventArgs e)
    {
        if (_contas.Count == 0)
        {
            await CarregarContasAsync();
            return;
        }

        EsconderStatus();

        foreach (var conta in _contas)
            conta.IniciarEdicao();

        AtualizarSaldosButton.IsVisible = false;
        SalvarButton.IsVisible = true;
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        EsconderStatus();

        // Valida tudo antes de enviar: o endpoint grava os saldos em lote, então uma
        // conta com valor inválido não pode deixar as outras irem pela metade.
        var invalidas = _contas.Where(c => !c.TentarAplicarTextoDigitado()).ToList();
        if (invalidas.Count > 0)
        {
            MostrarStatus($"Saldo inválido em: {string.Join(", ", invalidas.Select(c => c.Nome))}.", erro: true);
            return;
        }

        SetBusy(true);
        var resultado = await _contasApi.AtualizarSaldosAsync(_contas.Select(c => c.ParaDto()).ToList());
        SetBusy(false);

        if (!resultado.Sucesso)
        {
            MostrarStatus(resultado.Erro ?? "Não foi possível atualizar os saldos.", erro: true);
            return;
        }

        AtualizarTotal();
        SairDaEdicao();
        MostrarStatus("Saldos atualizados com sucesso.", erro: false);
    }

    private void SairDaEdicao()
    {
        foreach (var conta in _contas)
            conta.EncerrarEdicao();

        AtualizarSaldosButton.IsVisible = true;
        SalvarButton.IsVisible = false;
    }

    private void AtualizarTotal() =>
        TotalLabel.Text = _contas.Sum(c => c.Saldo).ToString("C", Moeda);

    private void SetBusy(bool busy)
    {
        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
        AtualizarSaldosButton.IsEnabled = !busy;
        SalvarButton.IsEnabled = !busy;
    }

    private void MostrarStatus(string mensagem, bool erro)
    {
        StatusLabel.Text = mensagem;
        StatusLabel.TextColor = erro ? Colors.OrangeRed : Colors.LightGreen;
        StatusLabel.IsVisible = true;
    }

    private void EsconderStatus() => StatusLabel.IsVisible = false;
}
