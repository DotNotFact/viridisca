using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlotsByCourseInstance;

public sealed record GetScheduleSlotsByCourseInstanceQuery(Guid CourseInstanceUid) : IRequest<List<ScheduleSlotDto>>;
