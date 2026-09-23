using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Contas_App.Services;

/// <summary>
/// Wrapper único sobre o HttpClient da API. Anexa o Bearer token por requisição e traduz
/// a resposta para <see cref="ApiResultado"/> — mesma divisão usada no Contas_Web, com o
/// tratamento de falha de conexão que o app precisa (celular sem rede).
/// </summary>
public class ApiClient(AppSession session)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };

    /// <summary>Retorna null quando a requisição falha — o chamador distingue "falhou" de "lista vazia".</summary>
    public async Task<T?> ObterAsync<T>(string url)
    {
        try
        {
            using var request = CriarRequisicao(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return default;
        }
    }

    public Task<ApiResultado> AdicionarAsync<TCorpo>(string url, TCorpo corpo) =>
        EnviarAsync(HttpMethod.Post, url, corpo);

    public Task<ApiResultado> AtualizarAsync<TCorpo>(string url, TCorpo corpo) =>
        EnviarAsync(HttpMethod.Put, url, corpo);

    private async Task<ApiResultado> EnviarAsync<TCorpo>(HttpMethod metodo, string url, TCorpo? corpo)
    {
        try
        {
            using var request = CriarRequisicao(metodo, url);
            if (corpo is not null)
                request.Content = JsonContent.Create(corpo);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode
                ? ApiResultado.Ok()
                : ApiResultado.Falha(await LerMensagemErroAsync(response));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ApiResultado.Falha("Não foi possível conectar ao servidor. Verifique sua conexão.");
        }
    }

    private HttpRequestMessage CriarRequisicao(HttpMethod metodo, string url)
    {
        var request = new HttpRequestMessage(metodo, url);
        if (session.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        return request;
    }

    // As validações de Biz da API voltam como BadRequest(string) com a mensagem pronta
    // para exibir; ProblemDetails (JSON) e os outros status viram mensagem genérica.
    private static async Task<string> LerMensagemErroAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var corpo = (await response.Content.ReadAsStringAsync()).Trim();
            if (corpo.Length > 0 && !corpo.StartsWith('{'))
                return corpo;

            return "Dados inválidos. Verifique os campos e tente novamente.";
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sessão expirada. Entre novamente.",
            HttpStatusCode.NotFound => "Registro não encontrado.",
            _ => "Não foi possível concluir a operação. Tente novamente."
        };
    }
}

public record ApiResultado(bool Sucesso, string? Erro)
{
    public static ApiResultado Ok() => new(true, null);

    public static ApiResultado Falha(string erro) => new(false, erro);
}
