using Contas_Db.Model.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contas_Db.Model;

[Table("tb_parcela")]
public class Parcela : ISoftDelete
{
    [Key]
    [Column("id_parcela")]
    public int Id { get; set; }

    [Required]
    [Column("id_divida")]
    public int IdDivida { get; set; }

    [ForeignKey(nameof(IdDivida))]
    public Divida? Divida { get; set; }

    [Required]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [Required]
    [Column("id_conta")]
    public int IdConta { get; set; }

    [ForeignKey(nameof(IdConta))]
    public Conta? Conta { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("ds_parcela")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Column("nr_valor", TypeName = "numeric(10,2)")]
    public decimal Valor { get; set; }

    [Required]
    [Column("dt_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("dt_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Required]
    [Column("dm_ativo")]
    public bool Ativo { get; set; }

    [NotMapped]
    public bool Pago => DataPagamento.HasValue;
}
