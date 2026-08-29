using ViridiscaUi.Domain.Entities.Auth; 

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Интерфейс сервиса для работы с людьми (Person)
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Получает человека по UID
    /// </summary>
    Task<Person?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Получает всех людей
    /// </summary>
    Task<IEnumerable<Person>> GetAllAsync();

    /// <summary>
    /// Получает всех людей (алиас для GetAllAsync)
    /// </summary>
    Task<IEnumerable<Person>> GetAllPersonsAsync();

    /// <summary>
    /// Создает нового человека
    /// </summary>
    Task<Person> CreateAsync(Person person);

    /// <summary>
    /// Обновляет человека
    /// </summary>
    Task<bool> UpdateAsync(Person person);

    /// <summary>
    /// Удаляет человека
    /// </summary>
    Task<bool> DeleteAsync(Guid uid);

    /// <summary>
    /// Получает человека по email
    /// </summary>
    Task<Person?> GetByEmailAsync(string email);

    /// <summary>
    /// Получает человека по телефону
    /// </summary>
    Task<Person?> GetByPhoneAsync(string phone);

    /// <summary>
    /// Ищет людей по имени
    /// </summary>
    Task<IEnumerable<Person>> SearchPersonsAsync(string searchTerm);

    /// <summary>
    /// Получает людей с пагинацией
    /// </summary>
    Task<(IEnumerable<Person> Persons, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Проверяет существование человека по email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeUid = null);

    /// <summary>
    /// Проверяет существование человека по телефону
    /// </summary>
    Task<bool> ExistsByPhoneAsync(string phone, Guid? excludeUid = null);

    /// <summary>
    /// Получает аккаунт человека
    /// </summary>
    Task<Account?> GetPersonAccountAsync(Guid personUid);

    /// <summary>
    /// Получает человека по электронной почте
    /// </summary>
    Task<Person?> GetPersonByEmailAsync(string email);

    /// <summary>
    /// Получает людей по роли
    /// </summary>
    Task<IEnumerable<Person>> GetPersonsByRoleAsync(string roleName);

    /// <summary>
    /// Деактивирует аккаунт пользователя (мягкое удаление)
    /// </summary>
    Task<bool> DeactivateAsync(Guid uid);
} 