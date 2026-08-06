namespace Contas_Contratos.Dto;

public class OperacaoDto
{
    public int Id { get; set; }
    public int IdInvestimento { get; set; }
    public bool Compra { get; set; }
    public DateTime DataOperacao { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorOperacao { get; set; }
    public bool Ativo { get; set; }
}
