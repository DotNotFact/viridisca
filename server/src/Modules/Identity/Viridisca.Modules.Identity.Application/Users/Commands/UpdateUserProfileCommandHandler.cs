using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Application.Exceptions;
using Viridisca.Common.Application.Identity;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;

namespace Viridisca.Modules.Identity.Application.Users.Commands;

public sealed class UpdateUserProfileCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserProfileCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        Guid userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("No authenticated user");

        User user = await _userRepository.GetByUidAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{userId}' was not found");

        var updateResult = user.UpdatePersonalInfo(
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.PhoneNumber);

        if (updateResult.IsFailure)
        {
            throw new ConflictException(updateResult.Error.Description);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
