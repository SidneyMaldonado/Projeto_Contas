namespace Contas_Core.Dto;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[]? Imagem { get; set; }
    public bool Ativo { get; set; }
}
