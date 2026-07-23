namespace Contas_Core.Dto;

public class DividaDto
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int? IdCredor { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int DiaVencimento { get; set; }
    public DateTime DataPrimeiroVencimento { get; set; }
    public int Parcelas { get; set; }
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
}
