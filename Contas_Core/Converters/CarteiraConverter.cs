using Contas_Core.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class CarteiraConverter
{
    public static Carteira ToEntity(AdicionarCarteiraDto dto) => new()
    {
        Nome = dto.Nome,
        Ativo = true
    };

    public static void ApplyUpdate(Carteira entity, AtualizarCarteiraDto dto)
    {
        entity.Nome = dto.Nome;
    }

    public static CarteiraDto ToDto(Carteira entity) => new()
    {
        Id = entity.Id,
        IdUsuario = entity.IdUsuario,
        Nome = entity.Nome,
        Ativo = entity.Ativo
    };

    public static IEnumerable<CarteiraDto> ToDto(IEnumerable<Carteira> entities) =>
        entities.Select(ToDto);
}
