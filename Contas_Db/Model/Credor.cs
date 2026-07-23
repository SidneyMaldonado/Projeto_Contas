using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_credor")]
public class Credor : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    public string? Observacoes { get; set; }

    public byte[]? Logo { get; set; }

    [Required]
    public bool Ativo { get; set; }
}
