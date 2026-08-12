using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class ContasApiService(HttpClient httpClient, AuthSession authSession)
{
    public async Task<IEnumerable<ContaResumoDto>?> ObterResumoAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/contas/resumo");
        if (authSession.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<IEnumerable<ContaResumoDto>>();
    }

    public async Task<bool> AtualizarSaldosAsync(IEnumerable<ContaResumoDto> contas)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "api/contas/saldos")
        {
            Content = JsonContent.Create(contas)
        };
        if (authSession.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);

        var response = await httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
