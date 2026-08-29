using System.Data.Common;
using Npgsql;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Application.Common.Interfaces;
using Viridisca.Modules.Academic.Application.Common.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Services;

/// <summary>
/// Reads user data from Identity's tables directly (same physical Postgres database,
/// "identity" schema) rather than via a project reference to the Identity module —
/// Academic and Identity stay independently deployable/compilable, sharing only the DB.
/// </summary>
public class UserInfoService(IDbConnectionFactory connectionFactory) : IUserInfoService
{
    private const string SelectUserSql = """
        select uid, email, username, first_name, last_name, middle_name,
               phone_number, profile_image_url, date_of_birth, is_active
        from identity.users
        where uid = @uid
        """;

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<UserInfoDto> GetUserInfoAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(SelectUserSql, (NpgsqlConnection)connection);
        command.Parameters.AddWithValue("uid", userUid);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserInfoDto
        {
            Uid = reader.GetGuid(0),
            Email = reader.GetString(1),
            Username = reader.GetString(2),
            FirstName = reader.GetString(3),
            LastName = reader.GetString(4),
            MiddleName = reader.IsDBNull(5) ? null : reader.GetString(5),
            PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
            ProfileImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
            DateOfBirth = reader.GetDateTime(8),
            IsActive = reader.GetBoolean(9),
        };
    }

    public async Task<string> GetUserFullNameAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        UserInfoDto user = await GetUserInfoAsync(userUid, cancellationToken);
        return user?.FullName;
    }

    public async Task<string> GetUserEmailAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        UserInfoDto user = await GetUserInfoAsync(userUid, cancellationToken);
        return user?.Email;
    }

    public async Task<string> GetUserPhoneAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        UserInfoDto user = await GetUserInfoAsync(userUid, cancellationToken);
        return user?.PhoneNumber;
    }

    public async Task<bool> UserExistsAsync(Guid userUid, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("select exists(select 1 from identity.users where uid = @uid)", (NpgsqlConnection)connection);
        command.Parameters.AddWithValue("uid", userUid);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
