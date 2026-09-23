using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class CategoriasApiService(ApiClient api)
{
    private const string Recurso = "api/categorias";

    public Task<IEnumerable<CategoriaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<CategoriaDto>>(Recurso);

    public Task<CategoriaDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<CategoriaDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarCategoriaDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarCategoriaDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
