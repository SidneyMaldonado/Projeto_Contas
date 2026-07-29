using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AdicionarCarteiraDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}
