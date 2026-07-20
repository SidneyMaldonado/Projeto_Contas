namespace Contas_Core.Biz;

public class AdicionarContaBiz
{
    private Contas_Db.Model.Conta _entity = null!;

    public bool IsValid(Contas_Db.Model.Conta entity)
    {
        _entity = entity;
        return NomeNotNull() && SaldoValido();
    }

    public bool NomeNotNull() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length >= 3;

    public bool SaldoValido() => _entity.Saldo >= 0;
}
