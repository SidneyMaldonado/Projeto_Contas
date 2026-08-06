using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class InvestimentoConverter
{
    public static Investimento ToEntity(AdicionarInvestimentoDto dto) => new()
    {
        IdCarteira = dto.IdCarteira,
        Nome = dto.Nome,
        Quantidade = dto.Quantidade,
        Cotacao = dto.Cotacao,
        Observacao = dto.Observacao,
        Ativo = true
    };

    public static void ApplyUpdate(Investimento entity, AtualizarInvestimentoDto dto)
    {
        entity.IdCarteira = dto.IdCarteira;
        entity.Nome = dto.Nome;
        entity.Quantidade = dto.Quantidade;
        entity.Cotacao = dto.Cotacao;
        entity.Observacao = dto.Observacao;
    }

    public static InvestimentoDto ToDto(Investimento entity) => new()
    {
        Id = entity.Id,
        IdCarteira = entity.IdCarteira,
        Nome = entity.Nome,
        Quantidade = entity.Quantidade,
        Cotacao = entity.Cotacao,
        Observacao = entity.Observacao,
        Ativo = entity.Ativo
    };

    public static IEnumerable<InvestimentoDto> ToDto(IEnumerable<Investimento> entities) =>
        entities.Select(ToDto);
}
