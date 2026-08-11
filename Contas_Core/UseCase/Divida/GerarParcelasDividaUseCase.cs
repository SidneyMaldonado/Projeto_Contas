using Contas_Core.UseCase.Parcela;

namespace Contas_Core.UseCase.Divida;

public class GerarParcelasDividaUseCase
{
    private readonly AdicionarParcelaUseCase _adicionarParcela;

    public GerarParcelasDividaUseCase(AdicionarParcelaUseCase adicionarParcela)
    {
        _adicionarParcela = adicionarParcela;
    }

    public async Task ExecuteAsync(Contas_Db.Model.Divida divida)
    {
        var valorParcela = decimal.Round(divida.Valor / divida.Parcelas, 2, MidpointRounding.AwayFromZero);
        var valorUltimaParcela = divida.Valor - valorParcela * (divida.Parcelas - 1);

        for (var numero = 1; numero <= divida.Parcelas; numero++)
        {
            var ultima = numero == divida.Parcelas;

            await _adicionarParcela.ExecuteAsync(new Contas_Db.Model.Parcela
            {
                IdDivida = divida.Id,
                IdCategoria = divida.IdCategoria,
                IdConta = divida.IdConta,
                Descricao = $"{divida.Nome} - Parcela {numero}/{divida.Parcelas}",
                Valor = ultima ? valorUltimaParcela : valorParcela,
                DataVencimento = divida.DataPrimeiroVencimento.AddMonths(numero - 1),
                Ativo = true
            });
        }
    }
}
