namespace Contas_Core.Dto;

public class InvestimentoDto
{
    public int Id { get; set; }
    public int IdCarteira { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Cotacao { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}
