using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class ContasApiService(ApiClient api)
{
    private const string Recurso = "api/contas";

    public Task<IEnumerable<ContaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<ContaDto>>(Recurso);

    public Task<ContaDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<ContaDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarContaDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarContaDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");

    // Usados pelo dashboard (Home.razor), não pela listagem.
    public Task<IEnumerable<ContaResumoDto>?> ObterResumoAsync() =>
        api.ObterAsync<IEnumerable<ContaResumoDto>>($"{Recurso}/resumo");

    public async Task<bool> AtualizarSaldosAsync(IEnumerable<ContaResumoDto> contas) =>
        (await api.AtualizarAsync($"{Recurso}/saldos", contas)).Sucesso;
}
