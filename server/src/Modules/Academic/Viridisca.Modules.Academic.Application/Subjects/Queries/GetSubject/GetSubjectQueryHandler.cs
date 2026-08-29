using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject.Dto;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject;

internal sealed class GetSubjectQueryHandler : IRequestHandler<GetSubjectQuery, SubjectDto>
{
    private readonly ISubjectRepository _subjectRepository;

    public GetSubjectQueryHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<SubjectDto> Handle(GetSubjectQuery request, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByUidAsync(request.SubjectUid, cancellationToken)
            ?? throw new Exception($"Предмет с ID {request.SubjectUid} не найден");

        return new SubjectDto
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
        };
    }
}
