namespace Contas_Contratos.Dto;

public class CarteiraDto
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
