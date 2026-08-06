using Contas_Contratos.Dto;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Contas_Web.Services;

public class AuthSession(ProtectedSessionStorage sessionStorage)
{
    private const string StorageKey = "auth-session";

    public string? Token { get; private set; }
    public UsuarioDto? Usuario { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public async Task SignInAsync(string token, UsuarioDto usuario)
    {
        Token = token;
        Usuario = usuario;
        await sessionStorage.SetAsync(StorageKey, new StoredAuth(token, usuario));
    }

    public async Task SignOutAsync()
    {
        Token = null;
        Usuario = null;
        await sessionStorage.DeleteAsync(StorageKey);
    }

    // Every page navigation runs in a fresh circuit, so this must be called
    // (e.g. from OnInitializedAsync) before relying on IsAuthenticated.
    public async Task RestoreAsync()
    {
        if (IsAuthenticated)
            return;

        var result = await sessionStorage.GetAsync<StoredAuth>(StorageKey);
        if (result.Success && result.Value is not null)
        {
            Token = result.Value.Token;
            Usuario = result.Value.Usuario;
        }
    }

    private record StoredAuth(string Token, UsuarioDto Usuario);
}
