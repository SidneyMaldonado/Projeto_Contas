using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class HistoricosApiService(ApiClient api)
{
    private const string Recurso = "api/historicos";

    public Task<IEnumerable<HistoricoDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<HistoricoDto>>(Recurso);

    public Task<HistoricoDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<HistoricoDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarHistoricoDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarHistoricoDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
