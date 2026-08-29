using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для аутентификации и авторизации
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Аутентифицирует пользователя по логину и паролю
    /// </summary>
    Task<(bool Success, Person? Person, string ErrorMessage)> AuthenticateAsync(string username, string password);
    
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName);
    
    /// <summary>
    /// Регистрирует нового пользователя с указанной ролью
    /// </summary>
    Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid roleId);
    
    /// <summary>
    /// Выходит из системы
    /// </summary>
    Task LogoutAsync();
    
    /// <summary>
    /// Получает текущего аутентифицированного пользователя
    /// </summary>
    Task<Person?> GetCurrentPersonAsync();
    
    /// <summary>
    /// Получает текущего пользователя (алиас для GetCurrentPersonAsync)
    /// </summary>
    Task<object?> GetCurrentUserAsync();
    
    /// <summary>
    /// Получает идентификатор текущего аутентифицированного пользователя
    /// </summary>
    Task<Guid> GetCurrentPersonUidAsync();
    
    /// <summary>
    /// Проверяет доступ пользователя к определенному разрешению
    /// </summary>
    Task<bool> HasPermissionAsync(Guid personUid, string permissionName);
    
    /// <summary>
    /// Проверяет наличие определенной роли у пользователя
    /// </summary>
    Task<bool> IsInRoleAsync(Guid personUid, string roleName);
    
    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid personUid, string currentPassword, string newPassword);
    
    /// <summary>
    /// Запрашивает сброс пароля пользователя
    /// </summary>
    Task<bool> RequestPasswordResetAsync(string email);
    
    /// <summary>
    /// Сбрасывает пароль пользователя
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    
    /// <summary>
    /// Получает аккаунт по Person UID
    /// </summary>
    Task<Account?> GetAccountByPersonUidAsync(Guid personUid);
    
    /// <summary>
    /// Блокирует аккаунт пользователя
    /// </summary>
    Task<bool> LockAccountAsync(Guid personUid, string reason);
    
    /// <summary>
    /// Разблокирует аккаунт пользователя
    /// </summary>
    Task<bool> UnlockAccountAsync(Guid personUid);
}
