using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.Auth;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для управления сессией пользователя (Singleton)
/// С поддержкой реактивных уведомлений об изменениях
/// </summary>
public class PersonSessionService : IReactivePersonSessionService
{
    private Person? _currentPerson;
    private Account? _currentAccount;

    // Реактивный субъект для уведомления об изменениях CurrentPerson
    private readonly BehaviorSubject<Person?> _currentPersonSubject = new(null);

    public Person? CurrentPerson => _currentPerson;

    public Account? CurrentAccount => _currentAccount;

    /// <summary>
    /// Observable для отслеживания изменений CurrentPerson
    /// </summary>
    public IObservable<Person?> CurrentPersonObservable => _currentPersonSubject.AsObservable();

    public void SetCurrentPerson(Person? person)
    {
        _currentPerson = person;
        _currentPersonSubject.OnNext(person); // Уведомляем подписчиков об изменении
    }

    public void SetCurrentAccount(Account? account)
    {
        _currentAccount = account;
    }

    public void ClearSession()
    {
        _currentPerson = null;
        _currentAccount = null;
        _currentPersonSubject.OnNext(null); // Уведомляем подписчиков об очистке сессии
    }

    /// <summary>
    /// Очищает текущего пользователя (алиас для ClearSession)
    /// </summary>
    public void ClearCurrentPerson()
    {
        ClearSession();
    }

    public void Dispose()
    {
        _currentPersonSubject?.Dispose();
    }
}