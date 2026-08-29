using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с людьми (Person)
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class PersonService : IPersonService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PersonService> _logger;

    public PersonService(ApplicationDbContext dbContext, ILogger<PersonService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Базовые CRUD операции

    /// <summary>
    /// Получает человека по идентификатору
    /// </summary>
    public async Task<Person?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.Uid == uid && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting person {PersonUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает всех людей
    /// </summary>
    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all persons");
            throw;
        }
    }

    /// <summary>
    /// Получает всех людей (алиас для GetAllAsync)
    /// </summary>
    public async Task<IEnumerable<Person>> GetAllPersonsAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Создает нового человека
    /// </summary>
    public async Task<Person> CreateAsync(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        try
        {
            // Валидация
            await ValidatePersonAsync(person, true);

            person.Uid = Guid.NewGuid();
            person.CreatedAt = DateTime.UtcNow;
            person.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Persons.Add(person);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created person {PersonName} with UID {PersonUid}", 
                $"{person.FirstName} {person.LastName}", person.Uid);
            return person;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person {PersonName}", $"{person.FirstName} {person.LastName}");
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующего человека
    /// </summary>
    public async Task<bool> UpdateAsync(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        try
        {
            var existingPerson = await _dbContext.Persons.FindAsync(person.Uid);
            if (existingPerson == null || existingPerson.IsDeleted)
                return false;

            // Валидация
            await ValidatePersonAsync(person, false);

            // Обновляем поля
            existingPerson.FirstName = person.FirstName;
            existingPerson.LastName = person.LastName;
            existingPerson.MiddleName = person.MiddleName;
            existingPerson.Email = person.Email;
            existingPerson.PhoneNumber = person.PhoneNumber;
            existingPerson.DateOfBirth = person.DateOfBirth;
            existingPerson.ProfileImageUrl = person.ProfileImageUrl;
            existingPerson.Address = person.Address;
            existingPerson.IsActive = person.IsActive;
            existingPerson.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated person {PersonUid}", person.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person {PersonUid}", person.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет человека
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var person = await _dbContext.Persons.FindAsync(uid);
            if (person == null || person.IsDeleted)
                return false;

            // Мягкое удаление
            person.IsDeleted = true;
            person.DeletedAt = DateTime.UtcNow;
            person.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted person {PersonUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting person {PersonUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для Person

    /// <summary>
    /// Получает человека по email
    /// </summary>
    public async Task<Person?> GetPersonByEmailAsync(string email)
    {
        try
        {
            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower() && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting person by email {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Получает человека по email (алиас для интерфейса)
    /// </summary>
    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await GetPersonByEmailAsync(email);
    }

    /// <summary>
    /// Получает человека по телефону
    /// </summary>
    public async Task<Person?> GetByPhoneAsync(string phone)
    {
        try
        {
            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.PhoneNumber == phone && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting person by phone {Phone}", phone);
            throw;
        }
    }

    /// <summary>
    /// Получает людей по роли
    /// </summary>
    public async Task<IEnumerable<Person>> GetPersonsByRoleAsync(string roleName)
    {
        try
        {
            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .Where(p => p.PersonRoles.Any(pr => pr.Role.Name == roleName && pr.IsActive) && !p.IsDeleted)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting persons by role {RoleName}", roleName);
            throw;
        }
    }

    /// <summary>
    /// Получает людей по роли и контексту
    /// </summary>
    public async Task<IEnumerable<Person>> GetPersonsByRoleAndContextAsync(string roleName, string? context = null)
    {
        try
        {
            var query = _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .Where(p => p.PersonRoles.Any(pr => pr.Role.Name == roleName && pr.IsActive) && !p.IsDeleted);

            if (!string.IsNullOrEmpty(context))
            {
                query = query.Where(p => p.PersonRoles.Any(pr => pr.Context == context));
            }

            return await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting persons by role {RoleName} and context {Context}", roleName, context);
            throw;
        }
    }

    /// <summary>
    /// Назначает роль человеку
    /// </summary>
    public async Task<bool> AssignRoleAsync(Guid personUid, Guid roleUid, string? context = null, DateTime? expiresAt = null, Guid? assignedBy = null)
    {
        try
        {
            var personRole = new PersonRole
            {
                Uid = Guid.NewGuid(),
                PersonUid = personUid,
                RoleUid = roleUid,
                Context = context,
                ExpiresAt = expiresAt,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            _dbContext.PersonRoles.Add(personRole);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Assigned role {RoleUid} to person {PersonUid}", roleUid, personUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleUid} to person {PersonUid}", roleUid, personUid);
            return false;
        }
    }

    /// <summary>
    /// Отзывает роль у человека
    /// </summary>
    public async Task<bool> RevokeRoleAsync(Guid personUid, Guid roleUid, string? context = null)
    {
        try
        {
            var query = _dbContext.PersonRoles
                .Where(pr => pr.PersonUid == personUid && pr.RoleUid == roleUid && pr.IsActive);

            if (!string.IsNullOrEmpty(context))
            {
                query = query.Where(pr => pr.Context == context);
            }

            var personRoles = await query.ToListAsync();

            foreach (var personRole in personRoles)
            {
                personRole.IsActive = false;
                personRole.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Revoked role {RoleUid} from person {PersonUid}", roleUid, personUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking role {RoleUid} from person {PersonUid}", roleUid, personUid);
            return false;
        }
    }

    /// <summary>
    /// Получает роли человека
    /// </summary>
    public async Task<IEnumerable<PersonRole>> GetPersonRolesAsync(Guid personUid)
    {
        try
        {
            return await _dbContext.PersonRoles
                .Include(pr => pr.Role)
                .Where(pr => pr.PersonUid == personUid && pr.IsActive)
                .OrderBy(pr => pr.Role.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for person {PersonUid}", personUid);
            throw;
        }
    }

    /// <summary>
    /// Обновляет профиль человека
    /// </summary>
    public async Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string? middleName, string? phoneNumber, string? address)
    {
        try
        {
            var person = await _dbContext.Persons.FindAsync(uid);
            if (person == null || person.IsDeleted)
                return false;

            person.FirstName = firstName;
            person.LastName = lastName;
            person.MiddleName = middleName;
            person.PhoneNumber = phoneNumber;
            person.Address = address;
            person.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated profile for person {PersonUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for person {PersonUid}", uid);
            return false;
        }
    }

    /// <summary>
    /// Обновляет изображение профиля
    /// </summary>
    public async Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl)
    {
        try
        {
            var person = await _dbContext.Persons.FindAsync(uid);
            if (person == null || person.IsDeleted)
                return false;

            person.ProfileImageUrl = imageUrl;
            person.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated profile image for person {PersonUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile image for person {PersonUid}", uid);
            return false;
        }
    }

    /// <summary>
    /// Поиск людей
    /// </summary>
    public async Task<IEnumerable<Person>> SearchPersonsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .Where(p => !p.IsDeleted && (
                    p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    p.LastName.ToLower().Contains(lowerSearchTerm) ||
                    p.MiddleName != null && p.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                    p.Email.ToLower().Contains(lowerSearchTerm) ||
                    p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)
                ))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching persons with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает аккаунт человека
    /// </summary>
    public async Task<Account?> GetPersonAccountAsync(Guid personUid)
    {
        try
        {
            StatusLogger.LogInfo($"Getting account for person: {personUid}", nameof(PersonService));
            
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.PersonUid == personUid && !a.IsDeleted);
            
            if (account != null)
            {
                StatusLogger.LogInfo($"Account found for person: {personUid}", nameof(PersonService));
            }
            else
            {
                StatusLogger.LogWarning($"Account not found for person: {personUid}", nameof(PersonService));
            }
            
            return account;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting account for person {personUid}: {ex.Message}", nameof(PersonService));
            return null;
        }
    }

    /// <summary>
    /// Получает людей с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Person> Persons, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        try
        {
            var query = _dbContext.Persons
                .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                .Where(p => !p.IsDeleted);

            // Применяем поиск
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    p.LastName.ToLower().Contains(lowerSearchTerm) ||
                    p.MiddleName != null && p.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                    p.Email.ToLower().Contains(lowerSearchTerm)
                );
            }

            var totalCount = await query.CountAsync();

            var persons = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (persons, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged persons");
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование по email
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Persons
                .Where(p => p.Email.ToLower() == email.ToLower() && !p.IsDeleted);

            if (excludeUid.HasValue)
            {
                query = query.Where(p => p.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if person exists by email {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование по телефону
    /// </summary>
    public async Task<bool> ExistsByPhoneAsync(string phone, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Persons
                .Where(p => p.PhoneNumber == phone && !p.IsDeleted);

            if (excludeUid.HasValue)
            {
                query = query.Where(p => p.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if person exists by phone {Phone}", phone);
            throw;
        }
    }

    /// <summary>
    /// Деактивирует аккаунт пользователя (мягкое удаление)
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid personUid)
    {
        try
        {
            StatusLogger.LogInfo($"Deactivating person account: {personUid}", nameof(PersonService));
            
            var person = await _dbContext.Persons.FindAsync(personUid);
            if (person == null || person.IsDeleted)
            {
                StatusLogger.LogWarning($"Person not found or already deleted: {personUid}", nameof(PersonService));
                return false;
            }

            // Мягкое удаление Person
            person.IsDeleted = true;
            person.LastModifiedAt = DateTime.UtcNow;

            // Деактивируем связанный Account
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.PersonUid == personUid && !a.IsDeleted);
            
            if (account != null)
            {
                account.IsActive = false;
                account.IsDeleted = true;
                account.LastModifiedAt = DateTime.UtcNow;
                StatusLogger.LogInfo($"Account deactivated for person: {personUid}", nameof(PersonService));
            }

            // Деактивируем все роли
            var personRoles = await _dbContext.PersonRoles
                .Where(pr => pr.PersonUid == personUid && pr.IsActive)
                .ToListAsync();

            foreach (var role in personRoles)
            {
                role.IsActive = false;
                role.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            StatusLogger.LogSuccess($"Person account deactivated successfully: {personUid}", nameof(PersonService));
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error deactivating person {personUid}: {ex.Message}", nameof(PersonService));
            return false;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация человека
    /// </summary>
    private async Task ValidatePersonAsync(Person person, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(person.FirstName))
            errors.Add("Имя обязательно");

        if (string.IsNullOrWhiteSpace(person.LastName))
            errors.Add("Фамилия обязательна");

        if (string.IsNullOrWhiteSpace(person.Email))
            errors.Add("Email обязателен");

        // Проверка формата email
        if (!string.IsNullOrWhiteSpace(person.Email) && !IsValidEmail(person.Email))
            errors.Add("Неверный формат email");

        // Проверка уникальности email
        if (!string.IsNullOrWhiteSpace(person.Email))
        {
            var emailExists = await ExistsByEmailAsync(person.Email, isCreate ? null : person.Uid);
            if (emailExists)
                errors.Add("Пользователь с таким email уже существует");
        }

        // Проверка уникальности телефона
        if (!string.IsNullOrWhiteSpace(person.PhoneNumber))
        {
            var phoneExists = await ExistsByPhoneAsync(person.PhoneNumber, isCreate ? null : person.Uid);
            if (phoneExists)
                errors.Add("Пользователь с таким номером телефона уже существует");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Проверка валидности email
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
} 