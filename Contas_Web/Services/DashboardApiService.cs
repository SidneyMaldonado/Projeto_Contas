using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class DashboardApiService(HttpClient httpClient, AuthSession authSession)
{
    public async Task<DashboardResumoDto?> ObterResumoAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/dashboard/resumo");
        if (authSession.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<DashboardResumoDto>();
    }
}
