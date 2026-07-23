using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AtualizarCredorDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    public string? Observacoes { get; set; }

    public byte[]? Logo { get; set; }
}
