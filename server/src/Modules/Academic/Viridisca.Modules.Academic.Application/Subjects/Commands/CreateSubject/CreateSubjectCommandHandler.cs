using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Subjects.Commands.CreateSubject;

internal sealed class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, Guid>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubjectCommandHandler(ISubjectRepository subjectRepository, IUnitOfWork unitOfWork)
    {
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var exists = await _subjectRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (exists)
        {
            throw new Exception($"Предмет с кодом {request.Code} уже существует");
        }

        var subjectResult = Subject.Create(
            request.Name,
            request.Code,
            request.Description,
            request.Credits,
            request.Type,
            request.Difficulty,
            request.DepartmentUid,
            request.MinimumRequiredGrade);

        if (subjectResult.IsFailure)
        {
            throw new Exception(subjectResult.Error.Message);
        }

        var subject = subjectResult.Value;

        _subjectRepository.Insert(subject);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return subject.Uid;
    }
}
