using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_carteira")]
public class Carteira : ISoftDelete
{
    [Key]
    [Column("id_carteira")]
    public int Id { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nm_carteira")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
