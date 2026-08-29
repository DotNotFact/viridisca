using MediatR;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Users.Queries;

public sealed class GetUserProfileByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserProfileByIdQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserProfileResponse> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByUidAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found");

        return UserProfileMapper.Map(user);
    }
}
