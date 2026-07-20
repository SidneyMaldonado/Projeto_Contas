namespace Contas_Core.Dto;

public class ContaResumoDto
{
    public int Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
}
