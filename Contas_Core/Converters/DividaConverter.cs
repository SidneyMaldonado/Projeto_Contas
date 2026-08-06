using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Core.Converters;

public static class DividaConverter
{
    public static Divida ToEntity(AdicionarDividaDto dto) => new()
    {
        IdUsuario = dto.IdUsuario,
        IdCredor = dto.IdCredor,
        Nome = dto.Nome,
        DiaVencimento = dto.DiaVencimento,
        DataPrimeiroVencimento = dto.DataPrimeiroVencimento,
        Parcelas = dto.Parcelas,
        Valor = dto.Valor,
        Ativo = true
    };

    public static void ApplyUpdate(Divida entity, AtualizarDividaDto dto)
    {
        entity.IdUsuario = dto.IdUsuario;
        entity.IdCredor = dto.IdCredor;
        entity.Nome = dto.Nome;
        entity.DiaVencimento = dto.DiaVencimento;
        entity.DataPrimeiroVencimento = dto.DataPrimeiroVencimento;
        entity.Parcelas = dto.Parcelas;
        entity.Valor = dto.Valor;
    }

    public static DividaDto ToDto(Divida entity) => new()
    {
        Id = entity.Id,
        IdUsuario = entity.IdUsuario,
        IdCredor = entity.IdCredor,
        Nome = entity.Nome,
        DiaVencimento = entity.DiaVencimento,
        DataPrimeiroVencimento = entity.DataPrimeiroVencimento,
        Parcelas = entity.Parcelas,
        Valor = entity.Valor,
        Ativo = entity.Ativo
    };

    public static IEnumerable<DividaDto> ToDto(IEnumerable<Divida> entities) =>
        entities.Select(ToDto);
}
