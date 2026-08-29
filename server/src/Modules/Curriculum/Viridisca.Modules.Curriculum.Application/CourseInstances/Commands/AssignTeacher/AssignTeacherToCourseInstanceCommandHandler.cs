using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.AssignTeacher;

internal sealed class AssignTeacherToCourseInstanceCommandHandler : IRequestHandler<AssignTeacherToCourseInstanceCommand, bool>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignTeacherToCourseInstanceCommandHandler(ICourseInstanceRepository courseInstanceRepository, IUnitOfWork unitOfWork)
    {
        _courseInstanceRepository = courseInstanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignTeacherToCourseInstanceCommand request, CancellationToken cancellationToken)
    {
        var courseInstance = await _courseInstanceRepository.GetByUidAsync(request.CourseInstanceUid, cancellationToken)
            ?? throw new InvalidOperationException($"Экземпляр курса с ID {request.CourseInstanceUid} не найден");

        courseInstance.AssignTeacher(request.TeacherUid);
        _courseInstanceRepository.Update(courseInstance);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
