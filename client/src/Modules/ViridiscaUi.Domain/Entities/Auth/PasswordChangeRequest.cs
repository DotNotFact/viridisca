using System.ComponentModel.DataAnnotations;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Модель запроса на смену пароля
/// </summary>
public class PasswordChangeRequest
{
    /// <summary>
    /// Текущий пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Required(ErrorMessage = "Новый пароль обязателен")]
    [MinLength(8, ErrorMessage = "Новый пароль должен содержать минимум 8 символов")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    [Required(ErrorMessage = "Необходимо подтвердить новый пароль")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
} 