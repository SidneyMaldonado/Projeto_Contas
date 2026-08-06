using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class UsuarioConverter
{
    public static Usuario ToEntity(AdicionarUsuarioDto dto) => new()
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Senha = dto.Senha,
        Imagem = dto.Imagem,
        Ativo = true
    };

    public static void ApplyUpdate(Usuario entity, AtualizarUsuarioDto dto)
    {
        entity.Nome = dto.Nome;
        entity.Email = dto.Email;
        entity.Imagem = dto.Imagem;
    }

    public static UsuarioDto ToDto(Usuario entity) => new()
    {
        Id = entity.Id,
        Nome = entity.Nome,
        Email = entity.Email,
        Imagem = entity.Imagem,
        Ativo = entity.Ativo
    };

    public static IEnumerable<UsuarioDto> ToDto(IEnumerable<Usuario> entities) =>
        entities.Select(ToDto);
}
