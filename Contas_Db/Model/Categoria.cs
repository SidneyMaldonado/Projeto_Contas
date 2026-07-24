using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_categoria")]
public class Categoria : ISoftDelete
{
    [Key]
    [Column("id_categoria")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nm_categoria")]
    public string Nome { get; set; } = string.Empty;

    [Column("img_categoria")]
    public byte[]? Imagem { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
