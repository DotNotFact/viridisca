using System;
using MediatR;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject.Dto;

namespace Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject;

public sealed record GetSubjectQuery(Guid SubjectUid) : IRequest<SubjectDto>;
