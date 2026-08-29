namespace ViridiscaUi.Infrastructure.Services;

public class AuthTokenStore : IAuthTokenStore
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
    }
}
