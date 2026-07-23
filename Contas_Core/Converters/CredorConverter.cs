using Contas_Core.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class CredorConverter
{
    public static Credor ToEntity(AdicionarCredorDto dto) => new()
    {
        Nome = dto.Nome,
        Observacoes = dto.Observacoes,
        Logo = dto.Logo,
        Ativo = true
    };

    public static void ApplyUpdate(Credor entity, AtualizarCredorDto dto)
    {
        entity.Nome = dto.Nome;
        entity.Observacoes = dto.Observacoes;
        entity.Logo = dto.Logo;
    }

    public static CredorDto ToDto(Credor entity) => new()
    {
        Id = entity.Id,
        Nome = entity.Nome,
        Observacoes = entity.Observacoes,
        Logo = entity.Logo,
        Ativo = entity.Ativo
    };

    public static IEnumerable<CredorDto> ToDto(IEnumerable<Credor> entities) =>
        entities.Select(ToDto);
}
