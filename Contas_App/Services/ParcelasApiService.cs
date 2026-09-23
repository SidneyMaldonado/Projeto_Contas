using Contas_Contratos.Dto;

namespace Contas_App.Services;

public class ParcelasApiService(ApiClient api)
{
    private const string Recurso = "api/parcelas";

    /// <summary>Todas as parcelas das contas do usuário logado — a API não filtra por período.</summary>
    public Task<IEnumerable<ParcelaDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<ParcelaDto>>(Recurso);
}
