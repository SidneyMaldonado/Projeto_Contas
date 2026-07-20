namespace Contas_Core.Biz;

public class AdicionarDividaBiz
{
    private Contas_Db.Model.Divida _entity = null!;

    public bool IsValid(Contas_Db.Model.Divida entity)
    {
        _entity = entity;
        return NomeNotNull() && ValorPositivo() && DataVencimentoNotPast() && DiaVencimentoValido()
            && ParcelasValido() && DiaVencimentoConsistente();
    }

    public bool NomeNotNull() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length >= 3;

    public bool ValorPositivo() => _entity.Valor > 0;

    public bool DataVencimentoNotPast() => _entity.DataPrimeiroVencimento.Date >= DateTime.Today;

    public bool DiaVencimentoValido() => _entity.DiaVencimento is >= 1 and <= 31;

    public bool ParcelasValido() => _entity.Parcelas >= 1;

    public bool DiaVencimentoConsistente() => _entity.DataPrimeiroVencimento.Day == _entity.DiaVencimento;
}
