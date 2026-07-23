using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class PagarParcelaDto
{
    [Required]
    public DateTime DataPagamento { get; set; }
}
