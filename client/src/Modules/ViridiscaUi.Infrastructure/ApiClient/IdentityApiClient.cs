using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/identity/* endpoints (see server/src/Modules/Identity).
/// </summary>
public class IdentityApiClient(HttpClient httpClient, ILogger<IdentityApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, AuthResponseDto? Data, string? Error)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        => PostAsync<LoginRequestDto, AuthResponseDto>("api/identity/login", new LoginRequestDto(email, password), cancellationToken);

    public async Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<RegisterRequestDto, object>(
            "api/identity/register", new RegisterRequestDto(email, password, firstName, lastName), cancellationToken, expectBody: false);

        return (success, error);
    }

    public Task<(bool Success, AuthResponseDto? Data, string? Error)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => PostAsync<RefreshTokenRequestDto, AuthResponseDto>("api/identity/refresh-token", new RefreshTokenRequestDto(refreshToken), cancellationToken);

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var (success, _, _) = await PostAsync<LogoutRequestDto, object>(
            "api/identity/logout", new LogoutRequestDto(refreshToken), cancellationToken, expectBody: false);

        return success;
    }

    public async Task<(bool Success, UserProfileResponseDto? Data, string? Error)> GetCurrentProfileAsync(CancellationToken cancellationToken = default)
        => await GetAsync<UserProfileResponseDto>("api/identity/users/me", cancellationToken);

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<ChangePasswordRequestDto, object>(
            "api/identity/users/me/change-password",
            new ChangePasswordRequestDto(currentPassword, newPassword, confirmPassword),
            cancellationToken,
            expectBody: false);

        return (success, error);
    }
}
