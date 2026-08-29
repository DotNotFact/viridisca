using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

public sealed class SubmissionGradedDomainEvent : DomainEvent
{
    public Guid SubmissionUid { get; }
    public decimal Score { get; }

    public SubmissionGradedDomainEvent(Guid submissionUid, decimal score)
    {
        SubmissionUid = submissionUid;
        Score = score;
    }
}
