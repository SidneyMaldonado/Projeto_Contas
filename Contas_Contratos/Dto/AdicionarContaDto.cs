using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AdicionarContaDto
{
    [Required]
    public int IdUsuario { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public byte[]? Imagem { get; set; }

    [Required]
    public decimal Saldo { get; set; }
}
