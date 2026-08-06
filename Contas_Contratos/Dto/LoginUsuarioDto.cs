using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class LoginUsuarioDto
{
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}
