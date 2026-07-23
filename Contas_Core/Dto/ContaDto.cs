namespace Contas_Core.Dto;

public class ContaDto
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public byte[]? Imagem { get; set; }
    public decimal Saldo { get; set; }
    public bool Ativo { get; set; }
}
