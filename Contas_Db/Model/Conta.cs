using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_conta")]
public class Conta : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public byte[]? Imagem { get; set; }

    [Required]
    [Column(TypeName = "numeric(10,2)")]
    public decimal Saldo { get; set; }

    [Required]
    public bool Ativo { get; set; }
}
