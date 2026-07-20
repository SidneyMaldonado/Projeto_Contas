namespace Contas_Core.Biz;

public class AdicionarCredorBiz
{
    private Contas_Db.Model.Credor _entity = null!;

    public bool IsValid(Contas_Db.Model.Credor entity)
    {
        _entity = entity;
        return NomeNotNull();
    }

    public bool NomeNotNull() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length >= 3;
}
