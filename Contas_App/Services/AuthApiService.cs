using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Contas_App.Services;

public class AuthApiService
{
#if ANDROID
    private const string BaseUrl = "http://10.0.2.2:5210/";
#else
    private const string BaseUrl = "http://localhost:5210/";
#endif

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(BaseUrl) };

    private class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioInfo Usuario { get; set; } = new();
    }

    private class RegisterRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public async Task<LoginResult> LoginAsync(string email, string senha)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                new LoginRequest { Email = email, Senha = senha },
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return LoginResult.Fail("E-mail ou senha inválidos.");

            if (!response.IsSuccessStatusCode)
                return LoginResult.Fail($"Erro ao acessar o servidor ({(int)response.StatusCode}).");

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
            if (payload is null || string.IsNullOrEmpty(payload.Token))
                return LoginResult.Fail("Resposta inválida do servidor.");

            return LoginResult.Ok(payload.Token, payload.Usuario);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return LoginResult.Fail("Não foi possível conectar ao servidor. Verifique sua conexão.");
        }
    }

    public async Task<RegisterResult> RegisterAsync(string nome, string email, string senha)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/usuarios",
                new RegisterRequest { Nome = nome, Email = email, Senha = senha },
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadFromJsonAsync<string>(JsonOptions);
                return RegisterResult.Fail(message ?? "Não foi possível concluir o cadastro.");
            }

            if (!response.IsSuccessStatusCode)
                return RegisterResult.Fail($"Erro ao acessar o servidor ({(int)response.StatusCode}).");

            var usuario = await response.Content.ReadFromJsonAsync<UsuarioInfo>(JsonOptions);
            return usuario is null
                ? RegisterResult.Fail("Resposta inválida do servidor.")
                : RegisterResult.Ok(usuario);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return RegisterResult.Fail("Não foi possível conectar ao servidor. Verifique sua conexão.");
        }
    }
}
