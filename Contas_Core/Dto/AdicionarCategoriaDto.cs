using System.ComponentModel.DataAnnotations;

namespace Contas_Core.Dto;

public class AdicionarCategoriaDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public byte[]? Imagem { get; set; }
}
