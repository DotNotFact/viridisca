using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Groups;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.UpdateGroup;

internal sealed class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, bool>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGroupCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByUidAsync(request.GroupUid, cancellationToken)
            ?? throw new Exception($"Группа с ID {request.GroupUid} не найдена");

        group.UpdateDetails(request.Name, request.Description, request.MaxStudents);
        _groupRepository.Update(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
