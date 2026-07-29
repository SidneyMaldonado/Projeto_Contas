namespace Contas_Core.Biz;

public class AdicionarOperacaoBiz
{
    private Contas_Db.Model.Operacao _entity = null!;

    public bool IsValid(Contas_Db.Model.Operacao entity)
    {
        _entity = entity;
        return QuantidadeValida() && ValorOperacaoValido() && DataOperacaoValida();
    }

    public bool QuantidadeValida() => _entity.Quantidade > 0;

    public bool ValorOperacaoValido() => _entity.ValorOperacao > 0;

    public bool DataOperacaoValida() => _entity.DataOperacao != default;
}
