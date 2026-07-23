using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_divida")]
public class Divida : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int IdUsuario { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    public int? IdCredor { get; set; }

    [ForeignKey(nameof(IdCredor))]
    public Credor? Credor { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public int DiaVencimento { get; set; }

    [Required]
    public DateTime DataPrimeiroVencimento { get; set; }

    [Required]
    public int Parcelas { get; set; }

    [Required]
    [Column(TypeName = "numeric(10,2)")]
    public decimal Valor { get; set; }

    [Required]
    public bool Ativo { get; set; }
}
