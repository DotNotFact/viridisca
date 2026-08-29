using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Application.Subjects.Commands.UpdateSubject;

internal sealed class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, bool>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubjectCommandHandler(ISubjectRepository subjectRepository, IUnitOfWork unitOfWork)
    {
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByUidAsync(request.SubjectUid, cancellationToken)
            ?? throw new Exception($"Предмет с ID {request.SubjectUid} не найден");

        subject.UpdateInfo(request.Name, request.Description, request.Credits);
        _subjectRepository.Update(subject);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
