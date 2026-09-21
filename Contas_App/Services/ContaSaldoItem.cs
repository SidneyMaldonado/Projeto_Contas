using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Contas_Contratos.Dto;

namespace Contas_App.Services;

/// <summary>
/// Linha da tabela de saldos da MainPage. Alterna entre exibição (Label com o saldo
/// formatado) e edição (Entry com o valor digitável).
/// </summary>
public class ContaSaldoItem : INotifyPropertyChanged
{
    private static readonly CultureInfo Moeda = new("pt-BR");

    private decimal _saldo;
    private string _saldoTexto;
    private bool _emEdicao;

    public ContaSaldoItem(ContaResumoDto conta)
    {
        Codigo = conta.Codigo;
        Nome = conta.Nome;
        _saldo = conta.Saldo;
        _saldoTexto = FormatarParaEdicao(conta.Saldo);
    }

    public int Codigo { get; }

    public string Nome { get; }

    public decimal Saldo
    {
        get => _saldo;
        private set
        {
            if (_saldo == value)
                return;

            _saldo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SaldoFormatado));
        }
    }

    public string SaldoFormatado => _saldo.ToString("C", Moeda);

    /// <summary>Valor digitado no Entry enquanto <see cref="EmEdicao"/> é true.</summary>
    public string SaldoTexto
    {
        get => _saldoTexto;
        set
        {
            if (_saldoTexto == value)
                return;

            _saldoTexto = value;
            OnPropertyChanged();
        }
    }

    public bool EmEdicao
    {
        get => _emEdicao;
        private set
        {
            if (_emEdicao == value)
                return;

            _emEdicao = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EmExibicao));
        }
    }

    // O XAML não tem conversor de bool invertido pronto; expor o oposto evita criar um só para isso.
    public bool EmExibicao => !_emEdicao;

    public void IniciarEdicao()
    {
        SaldoTexto = FormatarParaEdicao(_saldo);
        EmEdicao = true;
    }

    public void EncerrarEdicao() => EmEdicao = false;

    /// <summary>
    /// Aplica ao saldo o que foi digitado. Retorna false — sem alterar o saldo — quando
    /// o texto não é um número válido.
    /// </summary>
    public bool TentarAplicarTextoDigitado()
    {
        if (!TentarConverter(SaldoTexto, out var valor))
            return false;

        Saldo = valor;
        SaldoTexto = FormatarParaEdicao(valor);
        return true;
    }

    public ContaResumoDto ParaDto() => new()
    {
        Codigo = Codigo,
        Nome = Nome,
        Saldo = _saldo
    };

    // Sem separador de milhar: o campo é para digitar, não para ler.
    private static string FormatarParaEdicao(decimal valor) => valor.ToString("0.00", Moeda);

    // Aceita "1234,56" (teclado pt-BR) e "1234.56" (o teclado numérico do Android nem sempre
    // oferece a vírgula). O separador presente no texto decide a cultura, então o mesmo
    // valor nunca é lido de dois jeitos.
    private static bool TentarConverter(string? texto, out decimal valor)
    {
        valor = 0m;

        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo))
            return false;

        var cultura = limpo.Contains(',') ? Moeda : CultureInfo.InvariantCulture;
        return decimal.TryParse(limpo, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, cultura, out valor);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propriedade = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propriedade));
}
