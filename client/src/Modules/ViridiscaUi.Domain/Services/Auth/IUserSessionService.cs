using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для управления сессией пользователя (Singleton)
/// </summary>
public interface IPersonSessionService
{
    /// <summary>
    /// Текущий пользователь
    /// </summary>
    Person? CurrentPerson { get; }
    
    /// <summary>
    /// Устанавливает текущего пользователя
    /// </summary>
    void SetCurrentPerson(Person? person);
    
    /// <summary>
    /// Очищает сессию пользователя
    /// </summary>
    void ClearSession();
    
    /// <summary>
    /// Получает аккаунт текущего пользователя
    /// </summary>
    Account? CurrentAccount { get; }
    
    /// <summary>
    /// Устанавливает аккаунт текущего пользователя
    /// </summary>
    void SetCurrentAccount(Account? account);

    /// <summary>
    /// Очищает текущего пользователя (алиас для ClearSession)
    /// </summary>
    void ClearCurrentPerson();
} 