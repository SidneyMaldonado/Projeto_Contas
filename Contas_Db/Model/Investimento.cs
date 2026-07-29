using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_investimento")]
public class Investimento : ISoftDelete
{
    [Key]
    [Column("id_investimento")]
    public int Id { get; set; }

    [Required]
    [Column("id_carteira")]
    public int IdCarteira { get; set; }

    [ForeignKey(nameof(IdCarteira))]
    public Carteira? Carteira { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nm_investimento")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("nr_quantidade", TypeName = "numeric(10,2)")]
    public decimal Quantidade { get; set; }

    [Required]
    [Column("vl_cotacao", TypeName = "numeric(10,2)")]
    public decimal Cotacao { get; set; }

    [MaxLength(500)]
    [Column("ds_observacao")]
    public string? Observacao { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
