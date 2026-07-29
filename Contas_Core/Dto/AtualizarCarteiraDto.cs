using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AtualizarCarteiraDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}
