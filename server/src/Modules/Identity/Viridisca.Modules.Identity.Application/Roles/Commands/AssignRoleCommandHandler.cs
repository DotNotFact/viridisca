using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Application.Exceptions;
using Viridisca.Common.Application.Identity;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Roles.Commands;

public sealed class AssignRoleCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRoleCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByUidAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found");

        Role role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleName}' was not found");

        UserRole existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleUid == role.Uid);

        if (existingUserRole is not null)
        {
            if (existingUserRole.IsActive)
            {
                throw new ConflictException($"User already has role '{request.RoleName}'");
            }

            var reactivateResult = existingUserRole.Activate();
            if (reactivateResult.IsFailure)
            {
                throw new ConflictException(reactivateResult.Error.Description);
            }
        }
        else
        {
            var userRoleResult = UserRole.Create(
                user.Uid,
                role.Uid,
                assignedByUserUid: _currentUserService.UserId,
                scopeUid: request.ScopeId);

            if (userRoleResult.IsFailure)
            {
                throw new ConflictException(userRoleResult.Error.Description);
            }

            var addRoleResult = user.AddRole(userRoleResult.Value);
            if (addRoleResult.IsFailure)
            {
                throw new ConflictException(addRoleResult.Error.Description);
            }

            _userRepository.AddUserRole(userRoleResult.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
