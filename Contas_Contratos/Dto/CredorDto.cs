namespace Contas_Contratos.Dto;

public class CredorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public byte[]? Logo { get; set; }
    public bool Ativo { get; set; }
}
