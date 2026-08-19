using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Contas_Web.Services;

/// <summary>
/// Wrapper único sobre o HttpClient da API. Anexa o Bearer token por requisição
/// (nunca via DelegatingHandler — ver spec/commit_3307b09.md) e traduz a resposta
/// para <see cref="ApiResultado"/>.
/// </summary>
public class ApiClient(HttpClient httpClient, AuthSession authSession)
{
    /// <summary>Retorna null quando a requisição falha — o chamador distingue "falhou" de "lista vazia".</summary>
    public async Task<T?> ObterAsync<T>(string url)
    {
        using var request = CriarRequisicao(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public Task<ApiResultado> AdicionarAsync<TCorpo>(string url, TCorpo corpo) =>
        EnviarAsync(HttpMethod.Post, url, corpo);

    public Task<ApiResultado> AtualizarAsync<TCorpo>(string url, TCorpo corpo) =>
        EnviarAsync(HttpMethod.Put, url, corpo);

    public Task<ApiResultado> PatchAsync(string url) =>
        EnviarAsync<object?>(HttpMethod.Patch, url, null);

    public Task<ApiResultado> PatchAsync<TCorpo>(string url, TCorpo corpo) =>
        EnviarAsync(HttpMethod.Patch, url, corpo);

    public Task<ApiResultado> ExcluirAsync(string url) =>
        EnviarAsync<object?>(HttpMethod.Delete, url, null);

    private async Task<ApiResultado> EnviarAsync<TCorpo>(HttpMethod metodo, string url, TCorpo? corpo)
    {
        using var request = CriarRequisicao(metodo, url);
        if (corpo is not null)
            request.Content = JsonContent.Create(corpo);

        var response = await httpClient.SendAsync(request);
        return response.IsSuccessStatusCode
            ? ApiResultado.Ok()
            : ApiResultado.Falha(await LerMensagemErroAsync(response));
    }

    private HttpRequestMessage CriarRequisicao(HttpMethod metodo, string url)
    {
        var request = new HttpRequestMessage(metodo, url);
        if (authSession.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);

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
