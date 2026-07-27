using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_conta")]
public class Conta : ISoftDelete
{
    [Key]
    [Column("id_conta")]
    public int Id { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nm_conta")]
    public string Nome { get; set; } = string.Empty;

    [Column("img_conta")]
    public byte[]? Imagem { get; set; }

    [Required]
    [Column("nr_saldo", TypeName = "numeric(10,2)")]
    public decimal Saldo { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
