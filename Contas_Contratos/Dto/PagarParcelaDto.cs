using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class PagarParcelaDto
{
    [Required]
    public DateTime DataPagamento { get; set; }
}
