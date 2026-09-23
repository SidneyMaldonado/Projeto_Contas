using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class InvestimentosApiService(ApiClient api)
{
    private const string Recurso = "api/investimentos";

    public Task<IEnumerable<InvestimentoDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<InvestimentoDto>>(Recurso);

    public Task<InvestimentoDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<InvestimentoDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarInvestimentoDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarInvestimentoDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
