using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup.Dto;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Groups.Queries.GetAllGroups;

internal sealed class GetAllGroupsQueryHandler : IRequestHandler<GetAllGroupsQuery, List<GroupDto>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAllGroupsQueryHandler(IGroupRepository groupRepository, IStudentRepository studentRepository)
    {
        _groupRepository = groupRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<GroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllAsync(cancellationToken);
        var result = new List<GroupDto>(groups.Count);

        foreach (var group in groups)
        {
            var studentsCount = (await _studentRepository.GetByGroupUidAsync(group.Uid, cancellationToken)).Count();

            result.Add(new GroupDto
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
            });
        }

        return result;
    }
}
