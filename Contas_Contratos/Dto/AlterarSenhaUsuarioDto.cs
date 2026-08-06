using System.ComponentModel.DataAnnotations;

namespace Contas_Contratos.Dto;

public class AlterarSenhaUsuarioDto
{
    [Required]
    [MaxLength(100)]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NovaSenha { get; set; } = string.Empty;
}
