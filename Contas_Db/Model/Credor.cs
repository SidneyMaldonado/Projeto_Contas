using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_credor")]
public class Credor : ISoftDelete
{
    [Key]
    [Column("id_credor")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nm_credor")]
    public string Nome { get; set; } = string.Empty;

    [Column("ds_observacoes")]
    public string? Observacoes { get; set; }

    [Column("img_logo")]
    public byte[]? Logo { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
