using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class CarteirasApiService(ApiClient api)
{
    private const string Recurso = "api/carteiras";

    public Task<IEnumerable<CarteiraDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<CarteiraDto>>(Recurso);

    public Task<CarteiraDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<CarteiraDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarCarteiraDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarCarteiraDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
