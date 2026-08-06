using System.Net;
using System.Net.Http.Json;
using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class AuthApiService(HttpClient httpClient)
{
    public async Task<AuthResult> LoginAsync(LoginUsuarioDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", dto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return AuthResult.Fail("E-mail ou senha invÃ¡lidos.");

            if (!response.IsSuccessStatusCode)
                return AuthResult.Fail($"Erro ao acessar o servidor ({(int)response.StatusCode}).");

            var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (payload is null || string.IsNullOrEmpty(payload.Token))
                return AuthResult.Fail("Resposta invÃ¡lida do servidor.");

            return AuthResult.Ok(payload);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return AuthResult.Fail("NÃ£o foi possÃ­vel conectar ao servidor. Verifique sua conexÃ£o.");
        }
    }
}

public class AuthResult
{
    public bool Success { get; private init; }
    public LoginResponseDto? Response { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static AuthResult Ok(LoginResponseDto response) => new() { Success = true, Response = response };

    public static AuthResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}
