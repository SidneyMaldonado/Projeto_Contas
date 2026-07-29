using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_historico")]
public class Historico : ISoftDelete
{
    [Key]
    [Column("id_historico")]
    public int Id { get; set; }

    [Required]
    [Column("dt_historico")]
    public DateTime DataHistorico { get; set; }

    [Required]
    [Column("id_investimento")]
    public int IdInvestimento { get; set; }

    [ForeignKey(nameof(IdInvestimento))]
    public Investimento? Investimento { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nm_investimento")]
    public string NomeInvestimento { get; set; } = string.Empty;

    [Required]
    [Column("nr_quantidade", TypeName = "numeric(10,2)")]
    public decimal Quantidade { get; set; }

    [Required]
    [Column("vl_cotacao", TypeName = "numeric(10,2)")]
    public decimal Cotacao { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("ds_observacao")]
    public string Observacao { get; set; } = string.Empty;

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
