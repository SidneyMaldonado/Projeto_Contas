namespace Contas_Core.Dto;

public class HistoricoDto
{
    public int Id { get; set; }
    public DateTime DataHistorico { get; set; }
    public int IdInvestimento { get; set; }
    public string NomeInvestimento { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Cotacao { get; set; }
    public string Observacao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
