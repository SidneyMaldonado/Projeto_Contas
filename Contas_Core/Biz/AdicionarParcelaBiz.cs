namespace Contas_Core.Biz;

public class AdicionarParcelaBiz
{
    private Contas_Db.Model.Parcela _entity = null!;

    public bool IsValid(Contas_Db.Model.Parcela entity)
    {
        _entity = entity;
        return ValorPositivo();
    }

    public bool ValorPositivo() => _entity.Valor > 0;
}
