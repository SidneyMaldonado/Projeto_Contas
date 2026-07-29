using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AdicionarInvestimentoDto
{
    [Required]
    public int IdCarteira { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public decimal Quantidade { get; set; }

    [Required]
    public decimal Cotacao { get; set; }

    [MaxLength(500)]
    public string? Observacao { get; set; }
}
