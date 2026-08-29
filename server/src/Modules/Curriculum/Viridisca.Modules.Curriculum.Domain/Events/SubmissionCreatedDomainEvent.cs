using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

public sealed class SubmissionCreatedDomainEvent : DomainEvent
{
    public Guid SubmissionUid { get; }

    public SubmissionCreatedDomainEvent(Guid submissionUid)
    {
        SubmissionUid = submissionUid;
    }
}
