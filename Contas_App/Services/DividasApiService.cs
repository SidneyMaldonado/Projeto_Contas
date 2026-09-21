using Contas_Contratos.Dto;

namespace Contas_App.Services;

public class DividasApiService(ApiClient api)
{
    private const string Recurso = "api/dividas";

    /// <summary>Dívidas e receitas do usuário logado; <see cref="DividaDto.EhDivida"/> separa as duas.</summary>
    public Task<IEnumerable<DividaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<DividaDto>>(Recurso);
}
