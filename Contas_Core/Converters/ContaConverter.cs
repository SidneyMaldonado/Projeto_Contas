using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class ContaConverter
{
    public static Conta ToEntity(AdicionarContaDto dto) => new()
    {
        IdUsuario = dto.IdUsuario,
        Nome = dto.Nome,
        Imagem = dto.Imagem,
        Saldo = dto.Saldo,
        Ativo = true
    };

    public static void ApplyUpdate(Conta entity, AtualizarContaDto dto)
    {
        entity.IdUsuario = dto.IdUsuario;
        entity.Nome = dto.Nome;
        entity.Imagem = dto.Imagem;
        entity.Saldo = dto.Saldo;
    }

    public static ContaDto ToDto(Conta entity) => new()
    {
        Id = entity.Id,
        IdUsuario = entity.IdUsuario,
        Nome = entity.Nome,
        Imagem = entity.Imagem,
        Saldo = entity.Saldo,
        Ativo = entity.Ativo
    };

    public static IEnumerable<ContaDto> ToDto(IEnumerable<Conta> entities) =>
        entities.Select(ToDto);
}
