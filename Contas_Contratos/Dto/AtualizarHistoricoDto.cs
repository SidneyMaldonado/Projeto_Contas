using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AtualizarHistoricoDto
{
    [Required]
    public int IdInvestimento { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomeInvestimento { get; set; } = string.Empty;

    [Required]
    public decimal Quantidade { get; set; }

    [Required]
    public decimal Cotacao { get; set; }

    [Required]
    [MaxLength(500)]
    public string Observacao { get; set; } = string.Empty;
}
