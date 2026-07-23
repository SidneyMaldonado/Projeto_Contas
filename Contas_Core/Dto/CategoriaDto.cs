namespace Contas_Core.Dto;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public byte[]? Imagem { get; set; }
    public bool Ativo { get; set; }
}
