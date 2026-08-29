using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAcademicPeriod;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAllAcademicPeriods;

internal sealed class GetAllAcademicPeriodsQueryHandler : IRequestHandler<GetAllAcademicPeriodsQuery, List<AcademicPeriodDto>>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;

    public GetAllAcademicPeriodsQueryHandler(IAcademicPeriodRepository academicPeriodRepository)
    {
        _academicPeriodRepository = academicPeriodRepository;
    }

    public async Task<List<AcademicPeriodDto>> Handle(GetAllAcademicPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _academicPeriodRepository.GetAllAsync(cancellationToken);
        return periods.Select(AcademicPeriodMapper.Map).ToList();
    }
}
