using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AtualizarParcelaDto
{
    [Required]
    public int IdDivida { get; set; }

    [Required]
    public int IdCategoria { get; set; }

    [Required]
    public int IdConta { get; set; }

    [Required]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }
}
