using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AtualizarDividaDto
{
    [Required]
    public int IdUsuario { get; set; }

    public int? IdCredor { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public int DiaVencimento { get; set; }

    [Required]
    public DateTime DataPrimeiroVencimento { get; set; }

    [Required]
    public int Parcelas { get; set; }

    [Required]
    public decimal Valor { get; set; }
}
