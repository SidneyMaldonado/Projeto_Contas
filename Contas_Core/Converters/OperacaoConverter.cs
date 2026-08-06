using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class OperacaoConverter
{
    public static Operacao ToEntity(AdicionarOperacaoDto dto) => new()
    {
        IdInvestimento = dto.IdInvestimento,
        Compra = dto.Compra,
        DataOperacao = dto.DataOperacao,
        Quantidade = dto.Quantidade,
        ValorOperacao = dto.ValorOperacao,
        Ativo = true
    };

    public static void ApplyUpdate(Operacao entity, AtualizarOperacaoDto dto)
    {
        entity.IdInvestimento = dto.IdInvestimento;
        entity.Compra = dto.Compra;
        entity.DataOperacao = dto.DataOperacao;
        entity.Quantidade = dto.Quantidade;
        entity.ValorOperacao = dto.ValorOperacao;
    }

    public static OperacaoDto ToDto(Operacao entity) => new()
    {
        Id = entity.Id,
        IdInvestimento = entity.IdInvestimento,
        Compra = entity.Compra,
        DataOperacao = entity.DataOperacao,
        Quantidade = entity.Quantidade,
        ValorOperacao = entity.ValorOperacao,
        Ativo = entity.Ativo
    };

    public static IEnumerable<OperacaoDto> ToDto(IEnumerable<Operacao> entities) =>
        entities.Select(ToDto);
}
