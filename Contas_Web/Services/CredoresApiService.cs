using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class CredoresApiService(ApiClient api)
{
    private const string Recurso = "api/credores";

    public Task<IEnumerable<CredorDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<CredorDto>>(Recurso);

    public Task<CredorDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<CredorDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarCredorDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarCredorDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
