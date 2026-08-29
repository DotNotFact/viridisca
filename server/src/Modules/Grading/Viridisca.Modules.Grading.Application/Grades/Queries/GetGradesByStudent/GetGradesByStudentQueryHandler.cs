using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Application.Grades.Queries.Dto;
using Viridisca.Modules.Grading.Application.Grades.Queries.GetGrade;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesByStudent;

internal sealed class GetGradesByStudentQueryHandler : IRequestHandler<GetGradesByStudentQuery, List<GradeDto>>
{
    private readonly IGradeRepository _gradeRepository;

    public GetGradesByStudentQueryHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<List<GradeDto>> Handle(GetGradesByStudentQuery request, CancellationToken cancellationToken)
    {
        var grades = await _gradeRepository.GetByStudentUidAsync(request.StudentUid, cancellationToken);
        return grades.Select(GradeMapper.Map).ToList();
    }
}
