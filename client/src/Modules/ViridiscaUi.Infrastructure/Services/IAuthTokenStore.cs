namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Holds the current session's JWT access/refresh tokens in memory (Singleton, mirrors IPersonSessionService).
/// </summary>
public interface IAuthTokenStore
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    void SetTokens(string accessToken, string refreshToken);

    void Clear();
}
