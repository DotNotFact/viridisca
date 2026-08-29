using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Commands.CreateSubmission;

internal sealed class CreateSubmissionCommandHandler : IRequestHandler<CreateSubmissionCommand, Guid>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubmissionCommandHandler(
        ISubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByUidAsync(request.AssignmentUid, cancellationToken)
            ?? throw new InvalidOperationException($"Задание с ID {request.AssignmentUid} не найдено");

        var existing = await _submissionRepository.GetByStudentAndAssignmentAsync(request.StudentUid, request.AssignmentUid, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Студент уже сдал работу по этому заданию");
        }

        var submissionResult = Submission.Create(request.AssignmentUid, request.StudentUid, request.Content, request.FilePath);
        if (submissionResult.IsFailure)
        {
            throw new InvalidOperationException(submissionResult.Error.Message);
        }

        var submission = submissionResult.Value;

        if (assignment.DueDate.HasValue && DateTime.UtcNow > assignment.DueDate.Value)
        {
            submission.MarkAsLate();
        }

        _submissionRepository.Insert(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return submission.Uid;
    }
}
