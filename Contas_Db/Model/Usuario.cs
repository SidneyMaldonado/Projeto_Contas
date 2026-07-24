using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_usuario")]
public class Usuario : ISoftDelete
{
    [Key]
    [Column("id_usuario")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nm_usuario")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("ds_email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("ds_senha")]
    public string Senha { get; set; } = string.Empty;

    [Column("img_usuario")]
    public byte[]? Imagem { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
