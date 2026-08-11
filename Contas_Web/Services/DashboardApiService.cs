using System.Net.Http.Json;
using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class DashboardApiService(HttpClient httpClient)
{
    public async Task<DashboardResumoDto?> ObterResumoAsync()
    {
        var response = await httpClient.GetAsync("api/dashboard/resumo");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<DashboardResumoDto>();
    }
}
