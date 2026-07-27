using System.Security.Claims;

namespace Contas_Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUsuarioId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Token não contém o identificador do usuário.");

        return int.Parse(claim.Value);
    }
}
