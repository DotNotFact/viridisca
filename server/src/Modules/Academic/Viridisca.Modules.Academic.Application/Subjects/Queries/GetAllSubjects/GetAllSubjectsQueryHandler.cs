using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject.Dto;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Subjects.Queries.GetAllSubjects;

internal sealed class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, List<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;

    public GetAllSubjectsQueryHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<List<SubjectDto>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _subjectRepository.GetAllAsync(cancellationToken);

        return subjects.Select(subject => new SubjectDto
        {
            Uid = subject.Uid,
            Name = subject.Name,
            Code = subject.Code,
            Description = subject.Description,
            Credits = subject.Credits,
            Type = subject.Type.ToString(),
            Difficulty = subject.Difficulty.ToString(),
            DepartmentUid = subject.DepartmentUid,
            MinimumRequiredGrade = subject.MinimumRequiredGrade,
            IsActive = subject.IsActive,
            CreatedAtUtc = subject.CreatedAtUtc
        }).ToList();
    }
}
