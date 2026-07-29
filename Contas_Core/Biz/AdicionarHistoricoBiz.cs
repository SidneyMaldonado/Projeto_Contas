namespace Contas_Core.Biz;

public class AdicionarHistoricoBiz
{
    private Contas_Db.Model.Historico _entity = null!;

    public bool IsValid(Contas_Db.Model.Historico entity)
    {
        _entity = entity;
        return NomeInvestimentoValido() && QuantidadeValida() && CotacaoValida() && ObservacaoValida();
    }

    public bool NomeInvestimentoValido() =>
        !string.IsNullOrWhiteSpace(_entity.NomeInvestimento) && _entity.NomeInvestimento.Length <= 50;

    public bool QuantidadeValida() => _entity.Quantidade >= 0;

    public bool CotacaoValida() => _entity.Cotacao >= 0;

    public bool ObservacaoValida() =>
        !string.IsNullOrWhiteSpace(_entity.Observacao) && _entity.Observacao.Length <= 500;
}
