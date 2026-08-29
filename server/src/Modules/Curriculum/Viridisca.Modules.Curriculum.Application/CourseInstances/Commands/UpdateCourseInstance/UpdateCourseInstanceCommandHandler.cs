using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.UpdateCourseInstance;

internal sealed class UpdateCourseInstanceCommandHandler : IRequestHandler<UpdateCourseInstanceCommand, bool>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseInstanceCommandHandler(ICourseInstanceRepository courseInstanceRepository, IUnitOfWork unitOfWork)
    {
        _courseInstanceRepository = courseInstanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCourseInstanceCommand request, CancellationToken cancellationToken)
    {
        var courseInstance = await _courseInstanceRepository.GetByUidAsync(request.CourseInstanceUid, cancellationToken)
            ?? throw new InvalidOperationException($"Экземпляр курса с ID {request.CourseInstanceUid} не найден");

        var result = courseInstance.UpdateDetails(request.Name, request.Description, request.MaxEnrollments);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        _courseInstanceRepository.Update(courseInstance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
