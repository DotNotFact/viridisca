using MediatR;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Users.Queries;

public sealed class GetUserProfileQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserProfileQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserProfileResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.Username}' was not found");

        return UserProfileMapper.Map(user);
    }
}

internal static class UserProfileMapper
{
    public static UserProfileResponse Map(User user) => new(
        user.Uid,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.MiddleName,
        user.PhoneNumber,
        user.ProfileImageUrl,
        user.DateOfBirth,
        user.IsEmailConfirmed,
        user.LastLoginAtUtc ?? DateTime.MinValue,
        user.UserRoles.Select(ur => ur.Role.RoleType.ToString()));
}
