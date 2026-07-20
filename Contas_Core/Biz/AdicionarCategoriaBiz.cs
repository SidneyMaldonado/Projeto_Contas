namespace Contas_Core.Biz;

public class AdicionarCategoriaBiz
{
    private Contas_Db.Model.Categoria _entity = null!;

    public bool IsValid(Contas_Db.Model.Categoria entity)
    {
        _entity = entity;
        return NomeNotNull();
    }

    public bool NomeNotNull() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length >= 3;
}
