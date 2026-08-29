using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IAuthService, calling the backend's Identity API instead of reading
/// ApplicationDbContext directly (see PROGRESS.md, Phase 1).
///
/// The backend's user model (User/Role/RefreshToken) is not the same shape as the
/// frontend's own domain model (Person/Account/PersonRole, built for direct EF access).
/// This class never touches ApplicationDbContext: Person/Account instances returned here
/// are POCOs assembled in memory from API responses, not EF-tracked entities.
///
/// Backend does not yet support (all return a clear failure/false, not silently succeed):
/// granular permission checks (HasPermissionAsync), password-reset-by-email flow,
/// and admin lock/unlock of another account. See PROGRESS.md Phase 1 for what's tracked.
/// </summary>
public class HttpAuthService(
    IdentityApiClient apiClient,
    IAuthTokenStore tokenStore,
    IPersonSessionService personSessionService,
    ILogger<HttpAuthService> logger) : IAuthService
{
    private readonly IdentityApiClient _apiClient = apiClient;
    private readonly IAuthTokenStore _tokenStore = tokenStore;
    private readonly IPersonSessionService _personSessionService = personSessionService;
    private readonly ILogger<HttpAuthService> _logger = logger;

    public async Task<(bool Success, Person? Person, string ErrorMessage)> AuthenticateAsync(string username, string password)
    {
        var (loginSuccess, tokens, loginError) = await _apiClient.LoginAsync(username, password);
        if (!loginSuccess || tokens is null)
        {
            return (false, null, loginError ?? "Authentication failed");
        }

        _tokenStore.SetTokens(tokens.AccessToken, tokens.RefreshToken);

        var (profileSuccess, profile, profileError) = await _apiClient.GetCurrentProfileAsync();
        if (!profileSuccess || profile is null)
        {
            _tokenStore.Clear();
            return (false, null, profileError ?? "Failed to load user profile after login");
        }

        Person person = MapToPerson(profile);
        _personSessionService.SetCurrentPerson(person);
        _personSessionService.SetCurrentAccount(MapToAccount(profile));

        return (true, person, string.Empty);
    }

    public async Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
    {
        var (registerSuccess, registerError) = await _apiClient.RegisterAsync(email, password, firstName, lastName);
        if (!registerSuccess)
        {
            return (false, null, registerError ?? "Registration failed");
        }

        // Backend's Register doesn't return user data, only 200 OK — log in right away
        // to get the real, backend-assigned Uid instead of fabricating one client-side.
        return await AuthenticateAsync(email, password);
    }

    public Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid roleId)
    {
        // Backend's /api/identity/register does not accept a role at registration time
        // (see PROGRESS.md Phase 1) — every new user is created with zero roles.
        _logger.LogWarning("RegisterAsync with roleId is not supported by the backend yet; ignoring roleId {RoleId}", roleId);
        return RegisterAsync(username, email, password, firstName, lastName);
    }

    public async Task LogoutAsync()
    {
        if (_tokenStore.RefreshToken is { Length: > 0 } refreshToken)
        {
            await _apiClient.LogoutAsync(refreshToken);
        }

        _tokenStore.Clear();
        _personSessionService.ClearSession();
    }

    public Task<Person?> GetCurrentPersonAsync() => Task.FromResult(_personSessionService.CurrentPerson);

    public Task<object?> GetCurrentUserAsync() => Task.FromResult<object?>(_personSessionService.CurrentPerson);

    public Task<Guid> GetCurrentPersonUidAsync() => Task.FromResult(_personSessionService.CurrentPerson?.Uid ?? Guid.Empty);

    public async Task<bool> HasPermissionAsync(Guid personUid, string permissionName)
    {
        // Backend has role claims (RoleType) but no granular permission system yet.
        _logger.LogDebug("HasPermissionAsync is not supported by the backend yet ({Permission})", permissionName);
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> IsInRoleAsync(Guid personUid, string roleName)
    {
        if (_personSessionService.CurrentPerson?.Uid != personUid)
        {
            return false;
        }

        var (success, profile, _) = await _apiClient.GetCurrentProfileAsync();
        return success && profile is not null
            && profile.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> ChangePasswordAsync(Guid personUid, string currentPassword, string newPassword)
    {
        var (success, error) = await _apiClient.ChangePasswordAsync(currentPassword, newPassword, newPassword);
        if (!success)
        {
            _logger.LogWarning("ChangePasswordAsync failed: {Error}", error);
        }

        return success;
    }

    public Task<bool> RequestPasswordResetAsync(string email)
    {
        _logger.LogWarning("RequestPasswordResetAsync is not supported by the backend yet");
        return Task.FromResult(false);
    }

    public Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        _logger.LogWarning("ResetPasswordAsync is not supported by the backend yet");
        return Task.FromResult(false);
    }

    public Task<Account?> GetAccountByPersonUidAsync(Guid personUid)
        => Task.FromResult(_personSessionService.CurrentPerson?.Uid == personUid ? _personSessionService.CurrentAccount : null);

    public Task<bool> LockAccountAsync(Guid personUid, string reason)
    {
        _logger.LogWarning("LockAccountAsync is not supported by the backend yet");
        return Task.FromResult(false);
    }

    public Task<bool> UnlockAccountAsync(Guid personUid)
    {
        _logger.LogWarning("UnlockAccountAsync is not supported by the backend yet");
        return Task.FromResult(false);
    }

    private static Person MapToPerson(UserProfileResponseDto profile) => new()
    {
        Uid = profile.UserId,
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        MiddleName = profile.MiddleName,
        Email = profile.Email,
        PhoneNumber = profile.PhoneNumber,
        DateOfBirth = profile.DateOfBirth == default ? null : profile.DateOfBirth,
        ProfileImageUrl = profile.ProfileImageUrl,
        IsActive = true,
    };

    private static Account MapToAccount(UserProfileResponseDto profile) => new()
    {
        PersonUid = profile.UserId,
        Username = profile.Username,
        IsEmailConfirmed = profile.IsEmailConfirmed,
        IsActive = true,
        LastLoginAt = profile.LastLoginAt == default ? null : profile.LastLoginAt,
    };
}
