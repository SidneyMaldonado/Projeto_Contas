using Contas_Contratos.Dto;

namespace Contas_App.Services;

public class ContasApiService(ApiClient api)
{
    private const string Recurso = "api/contas";

    /// <summary>Contas ativas do usuário logado com o saldo de cada uma.</summary>
    public Task<IEnumerable<ContaResumoDto>?> ObterResumoAsync() =>
        api.ObterAsync<IEnumerable<ContaResumoDto>>($"{Recurso}/resumo");

    public Task<ApiResultado> AtualizarSaldosAsync(IEnumerable<ContaResumoDto> contas) =>
        api.AtualizarAsync($"{Recurso}/saldos", contas);
}
