using System;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.Auth;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Расширенный интерфейс сервиса сессии с реактивной функциональностью для UI слоя
/// </summary>
public interface IReactivePersonSessionService : IPersonSessionService
{
    /// <summary>
    /// Observable для отслеживания изменений CurrentPerson
    /// </summary>
    IObservable<Person?> CurrentPersonObservable { get; }
} 