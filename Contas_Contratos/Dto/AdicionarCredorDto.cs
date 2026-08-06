using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AdicionarCredorDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    public string? Observacoes { get; set; }

    public byte[]? Logo { get; set; }
}
