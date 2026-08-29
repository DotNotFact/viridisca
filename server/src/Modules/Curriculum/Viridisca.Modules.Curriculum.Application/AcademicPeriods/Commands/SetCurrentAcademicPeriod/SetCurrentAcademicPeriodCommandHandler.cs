using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.SetCurrentAcademicPeriod;

internal sealed class SetCurrentAcademicPeriodCommandHandler : IRequestHandler<SetCurrentAcademicPeriodCommand, bool>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetCurrentAcademicPeriodCommandHandler(IAcademicPeriodRepository academicPeriodRepository, IUnitOfWork unitOfWork)
    {
        _academicPeriodRepository = academicPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SetCurrentAcademicPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _academicPeriodRepository.GetByUidAsync(request.AcademicPeriodUid, cancellationToken)
            ?? throw new InvalidOperationException($"Учебный период с ID {request.AcademicPeriodUid} не найден");

        // Only one period can be "current" at a time.
        var previouslyCurrent = await _academicPeriodRepository.GetCurrentAsync(cancellationToken);
        if (previouslyCurrent is not null && previouslyCurrent.Uid != period.Uid)
        {
            previouslyCurrent.SetAsCurrent(false);
            _academicPeriodRepository.Update(previouslyCurrent);
        }

        period.SetAsCurrent(true);
        _academicPeriodRepository.Update(period);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
