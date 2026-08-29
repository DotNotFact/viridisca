using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Application.Exceptions;
using Viridisca.Common.Application.Identity;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;
using Viridisca.Modules.Identity.Domain.Services;

namespace Viridisca.Modules.Identity.Application.Users.Commands;

public sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("No authenticated user");

        User user = await _userRepository.GetByUidAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{userId}' was not found");

        if (!_passwordHasher.VerifyPassword(passwordHash: user.PasswordHash, providedPassword: request.CurrentPassword))
        {
            throw new ConflictException("Current password is incorrect");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ConflictException("New password and confirmation do not match");
        }

        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        var updateResult = user.UpdatePassword(newPasswordHash);
        if (updateResult.IsFailure)
        {
            throw new ConflictException(updateResult.Error.Description);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
