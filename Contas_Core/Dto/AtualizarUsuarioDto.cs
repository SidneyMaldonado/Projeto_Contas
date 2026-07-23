using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AtualizarUsuarioDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public byte[]? Imagem { get; set; }
}
