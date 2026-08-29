using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Groups;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.CreateGroup;

internal sealed class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGroupCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var exists = await _groupRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
        {
            throw new Exception($"Группа с кодом {request.Code} уже существует");
        }

        var groupResult = Group.Create(
            request.Code,
            request.Name,
            request.Description,
            request.Year,
            request.StartDate,
            request.MaxStudents,
            request.DepartmentUid,
            request.CuratorUid);

        if (groupResult.IsFailure)
        {
            throw new Exception(groupResult.Error.Message);
        }

        var group = groupResult.Value;

        _groupRepository.Insert(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Uid;
    }
}
