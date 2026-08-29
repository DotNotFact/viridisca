using FluentValidation;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.CreateScheduleSlot;

public sealed class CreateScheduleSlotCommandValidator : AbstractValidator<CreateScheduleSlotCommand>
{
    public CreateScheduleSlotCommandValidator()
    {
        RuleFor(x => x.CourseInstanceUid).NotEmpty().WithMessage("ID экземпляра курса обязателен");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("Время окончания должно быть позже времени начала");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("Дата окончания действия слота должна быть позже даты начала");
    }
}
