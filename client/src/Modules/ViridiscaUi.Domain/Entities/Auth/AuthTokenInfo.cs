namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Модель с информацией о токене авторизации
/// </summary>
public class AuthTokenInfo
{
    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Токен обновления
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Время истечения токена
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Флаг действительности токена
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Создает новый экземпляр информации о токене
    /// </summary>
    public AuthTokenInfo()
    {
    }
     
    /// <summary>
    /// Создает новый экземпляр информации о токене
    /// </summary>
    public static AuthTokenInfo Create(string token, string refreshToken, DateTime expiresAt)
    {
        var tokenInfo = new AuthTokenInfo
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IsValid = true
        };
        return tokenInfo;
    }

    /// <summary>
    /// Инвалидирует токен
    /// </summary>
    public void Invalidate()
    {
        IsValid = false;
        Token = string.Empty;
        RefreshToken = string.Empty;
    }

    /// <summary>
    /// Проверяет, истек ли срок действия токена
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }
} 