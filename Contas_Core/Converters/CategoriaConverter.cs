using Contas_Core.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class CategoriaConverter
{
    public static Categoria ToEntity(AdicionarCategoriaDto dto) => new()
    {
        Nome = dto.Nome,
        Imagem = dto.Imagem,
        Ativo = true
    };

    public static void ApplyUpdate(Categoria entity, AtualizarCategoriaDto dto)
    {
        entity.Nome = dto.Nome;
        entity.Imagem = dto.Imagem;
    }

    public static CategoriaDto ToDto(Categoria entity) => new()
    {
        Id = entity.Id,
        Nome = entity.Nome,
        Imagem = entity.Imagem,
        Ativo = entity.Ativo
    };

    public static IEnumerable<CategoriaDto> ToDto(IEnumerable<Categoria> entities) =>
        entities.Select(ToDto);
}
