using MediatR;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Users.Queries;

public sealed class GetUsersByRoleQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersByRoleQuery, UsersListResponse>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UsersListResponse> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Role, ignoreCase: true, out RoleType roleType))
        {
            throw new KeyNotFoundException($"Role '{request.Role}' was not found");
        }

        IEnumerable<User> users = await _userRepository.GetUsersByRoleAsync(roleType, cancellationToken);

        List<UserProfileResponse> userProfiles = users.Select(UserProfileMapper.Map).ToList();

        return new UsersListResponse(userProfiles, userProfiles.Count);
    }
}
