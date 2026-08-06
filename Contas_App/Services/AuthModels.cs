using Contas_Contratos.Dto;

namespace Contas_App.Services;

public class LoginResult
{
    public bool Success { get; private init; }
    public string? Token { get; private init; }
    public UsuarioDto? Usuario { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static LoginResult Ok(string token, UsuarioDto usuario) =>
        new() { Success = true, Token = token, Usuario = usuario };

    public static LoginResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}

public class RegisterResult
{
    public bool Success { get; private init; }
    public UsuarioDto? Usuario { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static RegisterResult Ok(UsuarioDto usuario) =>
        new() { Success = true, Usuario = usuario };

    public static RegisterResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
