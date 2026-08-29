using ViridiscaUi.Domain.Services.Auth;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса хеширования паролей с использованием BCrypt
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    /// <summary>
    /// Хеширует пароль используя BCrypt
    /// </summary>
    /// <param name="password">Пароль для хеширования</param>
    /// <returns>Хешированный пароль</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Проверяет пароль против хеша
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <param name="hash">Хеш для проверки</param>
    /// <returns>True если пароль соответствует хешу</returns>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 