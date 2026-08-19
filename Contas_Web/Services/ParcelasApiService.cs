using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class ParcelasApiService(ApiClient api)
{
    private const string Recurso = "api/parcelas";

    public Task<IEnumerable<ParcelaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<ParcelaDto>>(Recurso);

    public Task<ParcelaDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<ParcelaDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarParcelaDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarParcelaDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> PagarAsync(int id, PagarParcelaDto dto) =>
        api.PatchAsync($"{Recurso}/{id}/pagar", dto);

    public Task<ApiResultado> DesfazerPagamentoAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/desfazer-pagamento");

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
