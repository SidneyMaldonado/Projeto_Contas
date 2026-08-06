using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AtualizarOperacaoDto
{
    [Required]
    public int IdInvestimento { get; set; }

    [Required]
    public bool Compra { get; set; }

    [Required]
    public DateTime DataOperacao { get; set; }

    [Required]
    public int Quantidade { get; set; }

    [Required]
    public decimal ValorOperacao { get; set; }
}
