using Contas_Contratos.Dto;
using Contas_Core.Converters;
using Contas_Core.UseCase.Carteira;
using Contas_Core.UseCase.Conta;
using Contas_Core.UseCase.Investimento;
using Contas_Core.UseCase.Parcela;

namespace Contas_Core.UseCase.Dashboard;

public class ObterResumoDashboardUseCase
{
    private const int QuantidadeProximasParcelas = 10;

    private readonly ObterTodosContaUseCase _obterTodosConta;
    private readonly ObterTodosParcelaUseCase _obterTodosParcela;
    private readonly ObterTodosInvestimentoUseCase _obterTodosInvestimento;
    private readonly ObterTodosCarteiraUseCase _obterTodosCarteira;

    public ObterResumoDashboardUseCase(
        ObterTodosContaUseCase obterTodosConta,
        ObterTodosParcelaUseCase obterTodosParcela,
        ObterTodosInvestimentoUseCase obterTodosInvestimento,
        ObterTodosCarteiraUseCase obterTodosCarteira)
    {
        _obterTodosConta = obterTodosConta;
        _obterTodosParcela = obterTodosParcela;
        _obterTodosInvestimento = obterTodosInvestimento;
        _obterTodosCarteira = obterTodosCarteira;
    }

    public async Task<DashboardResumoDto> ExecuteAsync(int idUsuario)
    {
        var minhasContasIds = (await _obterTodosConta.ExecuteAsync())
            .Where(c => c.IdUsuario == idUsuario)
            .ToList();

        var saldoTotalContas = minhasContasIds.Where(c => c.Ativo).Sum(c => c.Saldo);
        var idsContas = minhasContasIds.Select(c => c.Id).ToHashSet();

        var parcelasNaoPagas = (await _obterTodosParcela.ExecuteAsync())
            .Where(p => idsContas.Contains(p.IdConta) && !p.Pago)
            .OrderBy(p => p.DataVencimento)
            .ToList();

        var minhasCarteirasIds = (await _obterTodosCarteira.ExecuteAsync())
            .Where(c => c.IdUsuario == idUsuario)
            .Select(c => c.Id)
            .ToHashSet();

        var valorTotalInvestido = (await _obterTodosInvestimento.ExecuteAsync())
            .Where(i => i.Ativo && minhasCarteirasIds.Contains(i.IdCarteira))
            .Sum(i => i.Quantidade * i.Cotacao);

        return new DashboardResumoDto
        {
            SaldoTotalContas = saldoTotalContas,
            ProximasParcelas = ParcelaConverter.ToDto(parcelasNaoPagas.Take(QuantidadeProximasParcelas)),
            ValorTotalInvestido = valorTotalInvestido,
            ValorTotalDividasAbertas = parcelasNaoPagas.Sum(p => p.Valor)
        };
    }
}
