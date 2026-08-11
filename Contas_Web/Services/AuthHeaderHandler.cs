using System.Net.Http.Headers;

namespace Contas_Web.Services;

public class AuthHeaderHandler(AuthSession authSession) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (authSession.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);

        return base.SendAsync(request, cancellationToken);
    }
}
