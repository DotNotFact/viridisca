using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup.Dto;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup;

internal sealed class GetGroupQueryHandler : IRequestHandler<GetGroupQuery, GroupDto>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IStudentRepository _studentRepository;

    public GetGroupQueryHandler(IGroupRepository groupRepository, IStudentRepository studentRepository)
    {
        _groupRepository = groupRepository;
        _studentRepository = studentRepository;
    }

    public async Task<GroupDto> Handle(GetGroupQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByUidAsync(request.GroupUid, cancellationToken)
            ?? throw new Exception($"Группа с ID {request.GroupUid} не найдена");

        var studentsCount = (await _studentRepository.GetByGroupUidAsync(group.Uid, cancellationToken)).Count();

        return new GroupDto
        {
            Uid = group.Uid,
            Code = group.Code,
            Name = group.Name,
            Description = group.Description,
            Year = group.Year,
            StartDate = group.StartDate,
            EndDate = group.EndDate,
            MaxStudents = group.MaxStudents,
            CurrentStudentsCount = studentsCount,
            Status = group.Status.ToString(),
            CuratorUid = group.CuratorUid,
            DepartmentUid = group.DepartmentUid,
            CreatedAtUtc = group.CreatedAtUtc
        };
    }
}
