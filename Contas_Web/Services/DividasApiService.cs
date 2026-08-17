using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class DividasApiService(ApiClient api)
{
    private const string Recurso = "api/dividas";

    public Task<IEnumerable<DividaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<DividaDto>>(Recurso);

    public Task<DividaDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<DividaDto>($"{Recurso}/{id}");

    /// <summary>A API gera as parcelas automaticamente após criar a dívida (GerarParcelasDividaUseCase).</summary>
    public Task<ApiResultado> AdicionarAsync(AdicionarDividaDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    public Task<ApiResultado> AtualizarAsync(int id, AtualizarDividaDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
