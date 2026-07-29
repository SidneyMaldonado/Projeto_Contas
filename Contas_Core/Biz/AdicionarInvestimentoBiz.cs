namespace Contas_Core.Biz;

public class AdicionarInvestimentoBiz
{
    private Contas_Db.Model.Investimento _entity = null!;

    public bool IsValid(Contas_Db.Model.Investimento entity)
    {
        _entity = entity;
        return NomeValido() && QuantidadeValida() && CotacaoValida();
    }

    public bool NomeValido() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length <= 50;

    public bool QuantidadeValida() => _entity.Quantidade > 0;

    public bool CotacaoValida() => _entity.Cotacao >= 0;
}
