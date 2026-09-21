using Contas_Contratos.Dto;

namespace Contas_App.Services;

/// <summary>
/// Guarda o token JWT e o usuário logado enquanto o app está aberto — equivalente ao
/// AuthSession do Contas_Web. O <see cref="ApiClient"/> lê o token daqui a cada requisição.
/// </summary>
public class AppSession
{
    public string? Token { get; private set; }
    public UsuarioDto? Usuario { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public void SignIn(string token, UsuarioDto usuario)
    {
        Token = token;
        Usuario = usuario;
    }

    public void SignOut()
    {
        Token = null;
        Usuario = null;
    }
}
