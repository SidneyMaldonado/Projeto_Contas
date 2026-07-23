namespace Contas_Core.Dto;

public class ParcelaDto
{
    public int Id { get; set; }
    public int IdDivida { get; set; }
    public int IdCategoria { get; set; }
    public int IdConta { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public bool Pago { get; set; }
    public bool Ativo { get; set; }
}
