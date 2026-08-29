namespace ViridiscaUi.Infrastructure.ApiClient;

public sealed record LoginRequestDto(string Email, string Password);

public sealed record RegisterRequestDto(string Email, string Password, string FirstName, string LastName);

public sealed record RefreshTokenRequestDto(string RefreshToken);

public sealed record LogoutRequestDto(string RefreshToken);

public sealed record ChangePasswordRequestDto(string CurrentPassword, string NewPassword, string ConfirmPassword);

public sealed record AuthResponseDto(string AccessToken, string RefreshToken);

public sealed record UserProfileResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    string? ProfileImageUrl,
    DateTime DateOfBirth,
    bool IsEmailConfirmed,
    DateTime LastLoginAt,
    IReadOnlyCollection<string> Roles);
