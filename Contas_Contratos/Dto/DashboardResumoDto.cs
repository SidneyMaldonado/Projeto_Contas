namespace Contas_Contratos.Dto;

public class DashboardResumoDto
{
    public decimal SaldoTotalContas { get; set; }
    public IEnumerable<ParcelaDto> ProximasParcelas { get; set; } = [];
    public decimal ValorTotalInvestido { get; set; }
    public decimal ValorTotalDividasAbertas { get; set; }
    public decimal ValorTotalReceitasAbertas { get; set; }
}
