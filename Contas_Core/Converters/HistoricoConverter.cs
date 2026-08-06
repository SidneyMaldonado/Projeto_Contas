using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class HistoricoConverter
{
    public static Historico ToEntity(AdicionarHistoricoDto dto) => new()
    {
        IdInvestimento = dto.IdInvestimento,
        NomeInvestimento = dto.NomeInvestimento,
        Quantidade = dto.Quantidade,
        Cotacao = dto.Cotacao,
        Observacao = dto.Observacao,
        DataHistorico = DateTime.UtcNow,
        Ativo = true
    };

    public static void ApplyUpdate(Historico entity, AtualizarHistoricoDto dto)
    {
        // DataHistorico nÃ£o Ã© alterada: Ã© um registro de auditoria imutÃ¡vel.
        entity.IdInvestimento = dto.IdInvestimento;
        entity.NomeInvestimento = dto.NomeInvestimento;
        entity.Quantidade = dto.Quantidade;
        entity.Cotacao = dto.Cotacao;
        entity.Observacao = dto.Observacao;
    }

    public static HistoricoDto ToDto(Historico entity) => new()
    {
        Id = entity.Id,
        DataHistorico = entity.DataHistorico,
        IdInvestimento = entity.IdInvestimento,
        NomeInvestimento = entity.NomeInvestimento,
        Quantidade = entity.Quantidade,
        Cotacao = entity.Cotacao,
        Observacao = entity.Observacao,
        Ativo = entity.Ativo
    };

    public static IEnumerable<HistoricoDto> ToDto(IEnumerable<Historico> entities) =>
        entities.Select(ToDto);
}
