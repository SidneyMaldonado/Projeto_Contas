using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_parcela")]
public class Parcela : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int IdDivida { get; set; }

    [ForeignKey(nameof(IdDivida))]
    public Divida? Divida { get; set; }

    [Required]
    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [Required]
    public int IdConta { get; set; }

    [ForeignKey(nameof(IdConta))]
    public Conta? Conta { get; set; }

    [Required]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "numeric(10,2)")]
    public decimal Valor { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    [Required]
    public bool Ativo { get; set; }

    [NotMapped]
    public bool Pago => DataPagamento.HasValue;
}
