using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.UpdateAcademicPeriod;

internal sealed class UpdateAcademicPeriodCommandHandler : IRequestHandler<UpdateAcademicPeriodCommand, bool>
{
    private readonly IAcademicPeriodRepository _academicPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAcademicPeriodCommandHandler(IAcademicPeriodRepository academicPeriodRepository, IUnitOfWork unitOfWork)
    {
        _academicPeriodRepository = academicPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateAcademicPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _academicPeriodRepository.GetByUidAsync(request.AcademicPeriodUid, cancellationToken)
            ?? throw new InvalidOperationException($"Учебный период с ID {request.AcademicPeriodUid} не найден");

        var detailsResult = period.UpdateDetails(request.Name, request.Description);
        if (detailsResult.IsFailure)
        {
            throw new InvalidOperationException(detailsResult.Error.Message);
        }

        var datesResult = period.UpdateDates(request.StartDate, request.EndDate);
        if (datesResult.IsFailure)
        {
            throw new InvalidOperationException(datesResult.Error.Message);
        }

        _academicPeriodRepository.Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
