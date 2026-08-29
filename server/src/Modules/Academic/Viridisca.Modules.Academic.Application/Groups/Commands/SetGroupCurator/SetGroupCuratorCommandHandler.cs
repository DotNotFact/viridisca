using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.SetGroupCurator;

internal sealed class SetGroupCuratorCommandHandler : IRequestHandler<SetGroupCuratorCommand, bool>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetGroupCuratorCommandHandler(IGroupRepository groupRepository, ITeacherRepository teacherRepository, IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _teacherRepository = teacherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SetGroupCuratorCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByUidAsync(request.GroupUid, cancellationToken)
            ?? throw new Exception($"Группа с ID {request.GroupUid} не найдена");

        if (request.CuratorUid.HasValue)
        {
            var teacher = await _teacherRepository.GetByUidAsync(request.CuratorUid.Value, cancellationToken)
                ?? throw new Exception($"Преподаватель с ID {request.CuratorUid} не найден");
        }

        group.SetCurator(request.CuratorUid);
        _groupRepository.Update(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
