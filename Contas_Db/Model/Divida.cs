using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_divida")]
public class Divida : ISoftDelete
{
    [Key]
    [Column("id_divida")]
    public int Id { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    [Column("id_credor")]
    public int? IdCredor { get; set; }

    [ForeignKey(nameof(IdCredor))]
    public Credor? Credor { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nm_divida")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("dia_vencimento")]
    public int DiaVencimento { get; set; }

    [Required]
    [Column("dt_primeiro_vencimento")]
    public DateTime DataPrimeiroVencimento { get; set; }

    [Required]
    [Column("nr_parcelas")]
    public int Parcelas { get; set; }

    [Required]
    [Column("nr_valor", TypeName = "numeric(10,2)")]
    public decimal Valor { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }
}
