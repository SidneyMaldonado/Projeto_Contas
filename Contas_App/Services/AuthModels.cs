namespace Contas_App.Services;

public class UsuarioInfo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginResult
{
    public bool Success { get; private init; }
    public string? Token { get; private init; }
    public UsuarioInfo? Usuario { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static LoginResult Ok(string token, UsuarioInfo usuario) =>
        new() { Success = true, Token = token, Usuario = usuario };

    public static LoginResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}

public class RegisterResult
{
    public bool Success { get; private init; }
    public UsuarioInfo? Usuario { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static RegisterResult Ok(UsuarioInfo usuario) =>
        new() { Success = true, Usuario = usuario };

    public static RegisterResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
