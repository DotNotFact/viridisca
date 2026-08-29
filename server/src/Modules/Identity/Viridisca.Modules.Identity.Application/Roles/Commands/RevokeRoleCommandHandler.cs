using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Application.Exceptions;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Roles.Commands;

public sealed class RevokeRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeRoleCommand>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(RevokeRoleCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByUidAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found");

        Role role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleName}' was not found");

        UserRole userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleUid == role.Uid && ur.IsActive)
            ?? throw new KeyNotFoundException($"User does not have role '{request.RoleName}'");

        var deactivateResult = userRole.Deactivate();
        if (deactivateResult.IsFailure)
        {
            throw new ConflictException(deactivateResult.Error.Description);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
