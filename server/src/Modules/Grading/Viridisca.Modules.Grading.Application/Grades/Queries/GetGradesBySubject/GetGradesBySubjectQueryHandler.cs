using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;
using Viridisca.Modules.Grading.Application.Grades.Queries.GetGrade;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesBySubject;

internal sealed class GetGradesBySubjectQueryHandler : IRequestHandler<GetGradesBySubjectQuery, List<GradeDto>>
{
    private readonly IGradeRepository _gradeRepository;

    public GetGradesBySubjectQueryHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<List<GradeDto>> Handle(GetGradesBySubjectQuery request, CancellationToken cancellationToken)
    {
        var grades = await _gradeRepository.GetBySubjectUidAsync(request.SubjectUid, cancellationToken);
        return grades.Select(GradeMapper.Map).ToList();
    }
}
