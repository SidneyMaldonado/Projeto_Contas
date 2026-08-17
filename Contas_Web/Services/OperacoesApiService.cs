using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class OperacoesApiService(ApiClient api)
{
    private const string Recurso = "api/operacoes";

    public Task<IEnumerable<OperacaoDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<OperacaoDto>>(Recurso);

    public Task<OperacaoDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<OperacaoDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarOperacaoDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarOperacaoDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
