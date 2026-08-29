using System.Net.Http.Headers;
using ViridiscaUi.Infrastructure.Services;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Attaches the current session's access token (if any) to every outgoing request.
/// Endpoints that don't require authorization simply ignore the header.
/// </summary>
public class AuthorizationHandler(IAuthTokenStore tokenStore) : DelegatingHandler
{
    private readonly IAuthTokenStore _tokenStore = tokenStore;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokenStore.AccessToken is { Length: > 0 } accessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
