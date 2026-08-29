using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

public sealed class AcademicPeriodCreatedDomainEvent : DomainEvent
{
    public Guid AcademicPeriodUid { get; }

    public AcademicPeriodCreatedDomainEvent(Guid academicPeriodUid)
    {
        AcademicPeriodUid = academicPeriodUid;
    }
}
