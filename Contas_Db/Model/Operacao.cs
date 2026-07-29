using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_operacao")]
public class Operacao : ISoftDelete
{
    [Key]
    [Column("id_operacao")]
    public int Id { get; set; }

    [Required]
    [Column("id_investimento")]
    public int IdInvestimento { get; set; }

    [ForeignKey(nameof(IdInvestimento))]
    public Investimento? Investimento { get; set; }

    [Required]
    [Column("dm_compra")]
    public bool Compra { get; set; }

    [Required]
    [Column("dt_operacao")]
    public DateTime DataOperacao { get; set; }

    [Required]
    [Column("nr_quantidade")]
    public int Quantidade { get; set; }

    [Required]
    [Column("vl_operacao", TypeName = "numeric(10,2)")]
    public decimal ValorOperacao { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
