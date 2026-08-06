using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AtualizarCarteiraDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}
