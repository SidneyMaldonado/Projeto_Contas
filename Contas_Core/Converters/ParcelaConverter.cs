using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class ParcelaConverter
{
    public static Parcela ToEntity(AdicionarParcelaDto dto) => new()
    {
        IdDivida = dto.IdDivida,
        IdCategoria = dto.IdCategoria,
        IdConta = dto.IdConta,
        Descricao = dto.Descricao,
        Valor = dto.Valor,
        DataVencimento = dto.DataVencimento,
        Ativo = true
    };

    public static void ApplyUpdate(Parcela entity, AtualizarParcelaDto dto)
    {
        entity.IdDivida = dto.IdDivida;
        entity.IdCategoria = dto.IdCategoria;
        entity.IdConta = dto.IdConta;
        entity.Descricao = dto.Descricao;
        entity.Valor = dto.Valor;
        entity.DataVencimento = dto.DataVencimento;
    }

    public static ParcelaDto ToDto(Parcela entity) => new()
    {
        Id = entity.Id,
        IdDivida = entity.IdDivida,
        IdCategoria = entity.IdCategoria,
        IdConta = entity.IdConta,
        Descricao = entity.Descricao,
        Valor = entity.Valor,
        DataVencimento = entity.DataVencimento,
        DataPagamento = entity.DataPagamento,
        Pago = entity.Pago,
        Ativo = entity.Ativo
    };

    public static IEnumerable<ParcelaDto> ToDto(IEnumerable<Parcela> entities) =>
        entities.Select(ToDto);
}
