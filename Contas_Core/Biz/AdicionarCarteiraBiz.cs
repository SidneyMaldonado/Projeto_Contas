namespace Contas_Core.Biz;

public class AdicionarCarteiraBiz
{
    private Contas_Db.Model.Carteira _entity = null!;

    public bool IsValid(Contas_Db.Model.Carteira entity)
    {
        _entity = entity;
        return NomeValido();
    }

    public bool NomeValido() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length is >= 3 and <= 50;
}
