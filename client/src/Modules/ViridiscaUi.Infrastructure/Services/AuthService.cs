using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Сервис аутентификации для новой архитектуры Person/Account
/// </summary>
public class AuthService : IAuthService
{
    private readonly IPersonService _personService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;
    private readonly IPersonSessionService _personSessionService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHashingService _passwordHashingService;

    public AuthService(
        IPersonService personService,
        IPermissionService permissionService,
        IRoleService roleService,
        IPersonSessionService personSessionService,
        ApplicationDbContext dbContext,
        IPasswordHashingService passwordHashingService)
    {
        _personService = personService;
        _permissionService = permissionService;
        _roleService = roleService;
        _personSessionService = personSessionService;
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
    }

    /// <summary>
    /// Аутентифицирует пользователя по логину и паролю
    /// </summary>
    public async Task<(bool Success, Person? Person, string ErrorMessage)> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Ищем аккаунт по username
            var account = await _dbContext.Accounts
                .Include(a => a.Person)
                .ThenInclude(p => p.PersonRoles)
                .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(a => a.Username == username);

            if (account == null)
            {
                _personSessionService.ClearSession();
                return (false, null, "Пользователь не найден");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
            {
                // Увеличиваем счетчик неудачных попыток
                account.FailedLoginAttempts++;
                account.LastFailedLoginAt = DateTime.UtcNow;

                if (account.FailedLoginAttempts >= 5)
                {
                    account.IsLocked = true;
                    account.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }

                await _dbContext.SaveChangesAsync();
                _personSessionService.ClearSession();
                return (false, null, "Неверный пароль");
            }

            if (account.IsLocked && account.LockedUntil > DateTime.UtcNow)
            {
                _personSessionService.ClearSession();
                return (false, null, $"Учетная запись заблокирована до {account.LockedUntil:HH:mm dd.MM.yyyy}");
            }

            if (!account.IsActive)
            {
                _personSessionService.ClearSession();
                return (false, null, "Учетная запись деактивирована");
            }

            // Сбрасываем счетчик неудачных попыток при успешном входе
            account.FailedLoginAttempts = 0;
            account.LastLoginAt = DateTime.UtcNow;
            account.IsLocked = false;
            account.LockedUntil = null;
            await _dbContext.SaveChangesAsync();

            StatusLogger.LogInfo($"Пользователь успешно аутентифицирован: {account.Person.Email}", nameof(AuthService));

            _personSessionService.SetCurrentPerson(account.Person);
            _personSessionService.SetCurrentAccount(account);
            
            return (true, account.Person, string.Empty);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка аутентификации: {ex.Message}", nameof(AuthService));
            _personSessionService.ClearSession();
            return (false, null, $"Ошибка аутентификации: {ex.Message}");
        }
    }

    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    public async Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
    {
        // Используем роль студента по умолчанию
        var studentRole = await _roleService.GetRoleByNameAsync("Student");

        if (studentRole == null)
        {
            return (false, null, "Системная ошибка: роль студента не найдена");
        }

        return await RegisterAsync(username, email, password, firstName, lastName, studentRole.Uid);
    }

    /// <summary>
    /// Регистрирует нового пользователя с указанной ролью
    /// </summary>
    public async Task<(bool Success, Person? Person, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid roleId)
    {
        try
        {
            // Проверяем существование username
            var existingAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Username == username);

            if (existingAccount != null)
            {
                return (false, null, "Пользователь с таким именем уже существует");
            }

            // Проверяем существование email
            var existingPerson = await _personService.GetPersonByEmailAsync(email);
            if (existingPerson != null)
            {
                return (false, null, "Пользователь с таким email уже существует");
            }

            // Проверяем роль
            var role = await _roleService.GetRoleAsync(roleId);
            if (role == null)
            {
                return (false, null, "Указанная роль не найдена");
            }

            // Создаем Person
            var person = new Person
            {
                Uid = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Создаем Account
            var account = new Account
            {
                Uid = Guid.NewGuid(),
                PersonUid = person.Uid,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Создаем PersonRole
            var personRole = new PersonRole
            {
                Uid = Guid.NewGuid(),
                PersonUid = person.Uid,
                RoleUid = roleId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Сохраняем в базу
            _dbContext.Persons.Add(person);
            _dbContext.Accounts.Add(account);
            _dbContext.PersonRoles.Add(personRole);
            await _dbContext.SaveChangesAsync();

            return (true, person, string.Empty);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка регистрации: {ex.Message}", nameof(AuthService));
            return (false, null, $"Ошибка регистрации: {ex.Message}");
        }
    }

    /// <summary>
    /// Выходит из системы
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            StatusLogger.LogInfo("User logout initiated", nameof(AuthService));
            
            // Очищаем сессию пользователя
            _personSessionService.ClearSession();
            
            StatusLogger.LogSuccess("User logged out successfully", nameof(AuthService));
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error during logout: {ex.Message}", nameof(AuthService));
            throw;
        }
    }

    /// <summary>
    /// Получает текущего аутентифицированного пользователя
    /// </summary>
    public async Task<Person?> GetCurrentPersonAsync()
    {
        return _personSessionService.CurrentPerson;
    }

    /// <summary>
    /// Получает текущего пользователя (алиас для GetCurrentPersonAsync)
    /// </summary>
    public async Task<object?> GetCurrentUserAsync()
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return null;

        return new
        {
            PersonUid = person.Uid,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email,
            Roles = person.PersonRoles?.Where(pr => pr.IsActive)
                .Select(pr => pr.Role?.Name ?? "Unknown")
                .ToList() ?? new List<string>(),
            LastLoginAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Получает Uid текущего пользователя
    /// </summary>
    public async Task<Guid> GetCurrentPersonUidAsync()
    {
        var person = await GetCurrentPersonAsync();
        return person?.Uid ?? Guid.Empty;
    }

    /// <summary>
    /// Получает аккаунт по PersonUid
    /// </summary>
    public async Task<Account?> GetAccountByPersonUidAsync(Guid personUid)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.PersonUid == personUid);
    }

    /// <summary>
    /// Блокирует аккаунт
    /// </summary>
    public async Task<bool> LockAccountAsync(Guid personUid, string reason)
    {
        try
        {
            var account = await GetAccountByPersonUidAsync(personUid);
            if (account == null)
                return false;

            account.IsLocked = true;
            account.LockedUntil = DateTime.UtcNow.AddDays(30); // Блокируем на 30 дней
            account.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка блокировки аккаунта: {ex.Message}", nameof(AuthService));
            return false;
        }
    }

    /// <summary>
    /// Разблокирует аккаунт
    /// </summary>
    public async Task<bool> UnlockAccountAsync(Guid personUid)
    {
        try
        {
            var account = await GetAccountByPersonUidAsync(personUid);
            if (account == null)
                return false;

            account.IsLocked = false;
            account.LockedUntil = null;
            account.FailedLoginAttempts = 0;
            account.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка разблокировки аккаунта: {ex.Message}", nameof(AuthService));
            return false;
        }
    }

    /// <summary>
    /// Проверяет доступ пользователя к определенному разрешению
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid personUid, string permissionName)
    {
        var personRoles = await _dbContext.PersonRoles
            .Include(pr => pr.Role)
            .Where(pr => pr.PersonUid == personUid && pr.IsActive)
            .ToListAsync();

        // Получаем права доступа для ролей пользователя
        var roleUids = personRoles.Select(pr => pr.RoleUid).ToList();
        var hasPermission = await _dbContext.RolePermissions
            .Include(rp => rp.Permission)
            .AnyAsync(rp => roleUids.Contains(rp.RoleUid) && 
                           rp.Permission.Name == permissionName);

        return hasPermission;
    }

    /// <summary>
    /// Проверяет наличие определенной роли у пользователя
    /// </summary>
    public async Task<bool> IsInRoleAsync(Guid personUid, string roleName)
    {
        return await _dbContext.PersonRoles
            .Include(pr => pr.Role)
            .AnyAsync(pr => pr.PersonUid == personUid && 
                           pr.Role.Name == roleName && 
                           pr.IsActive);
    }

    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    /// <param name="personUid">UID пользователя</param>
    /// <param name="currentPassword">Текущий пароль</param>
    /// <param name="newPassword">Новый пароль</param>
    /// <returns>Результат операции</returns>
    public async Task<bool> ChangePasswordAsync(Guid personUid, string currentPassword, string newPassword)
    {
        try
        {
            StatusLogger.LogInfo($"Changing password for person: {personUid}", nameof(AuthService));

            // Получаем аккаунт пользователя
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.PersonUid == personUid && a.IsActive);

            if (account == null)
            {
                StatusLogger.LogWarning($"Account not found for person: {personUid}", nameof(AuthService));
                return false;
            }

            // Проверяем текущий пароль
            if (!_passwordHashingService.VerifyPassword(currentPassword, account.PasswordHash))
            {
                StatusLogger.LogWarning($"Current password verification failed for person: {personUid}", nameof(AuthService));
                return false;
            }

            // Устанавливаем новый пароль
            account.PasswordHash = _passwordHashingService.HashPassword(newPassword);
            account.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            StatusLogger.LogSuccess($"Password changed successfully for person: {personUid}", nameof(AuthService));
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error changing password for person {personUid}: {ex.Message}", nameof(AuthService));
            return false;
        }
    }

    /// <summary>
    /// Запрашивает сброс пароля пользователя
    /// </summary>
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            var person = await _personService.GetPersonByEmailAsync(email);
            if (person == null)
                return false;

            // TODO: Реализовать генерацию токена и отправку email
            StatusLogger.LogInfo($"Запрос сброса пароля для {email}", nameof(AuthService));
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка запроса сброса пароля: {ex.Message}", nameof(AuthService));
            return false;
        }
    }

    /// <summary>
    /// Сбрасывает пароль пользователя
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            // TODO: Реализовать проверку токена и сброс пароля
            StatusLogger.LogInfo("Сброс пароля по токену");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка сброса пароля: {ex.Message}", nameof(AuthService));
            return false;
        }
    }
}