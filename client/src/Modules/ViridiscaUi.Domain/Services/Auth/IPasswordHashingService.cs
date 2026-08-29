namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для хеширования паролей
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Хеширует пароль
    /// </summary>
    /// <param name="password">Пароль для хеширования</param>
    /// <returns>Хешированный пароль</returns>
    string HashPassword(string password);

    /// <summary>
    /// Проверяет пароль против хеша
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <param name="hash">Хеш для проверки</param>
    /// <returns>True если пароль соответствует хешу</returns>
    bool VerifyPassword(string password, string hash);
} 