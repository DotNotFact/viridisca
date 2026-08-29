using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Common;

/// <summary>
/// ViewModel для отображения и редактирования информации о персоне
/// </summary>
public class PersonViewModel : RoutableViewModelBase
{
    private readonly IPersonService _personService;

    /// <summary>
    /// Текущая персона
    /// </summary>
    [Reactive] public Person? Entity { get; set; }

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string? MiddleName { get; set; }
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string? PhoneNumber { get; set; }
    [Reactive] public DateTime? DateOfBirth { get; set; }
    [Reactive] public string? Address { get; set; }
    [Reactive] public string? ProfileImageUrl { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName) 
        ? $"{FirstName} {LastName}" 
        : $"{FirstName} {MiddleName} {LastName}";

    /// <summary>
    /// Краткое имя (Фамилия И.О.)
    /// </summary>
    public string ShortName
    {
        get
        {
            var firstInitial = !string.IsNullOrEmpty(FirstName) ? FirstName[0] + "." : "";
            var middleInitial = !string.IsNullOrEmpty(MiddleName) ? MiddleName[0] + "." : "";
            return $"{LastName} {firstInitial}{middleInitial}".Trim();
        }
    }

    /// <summary>
    /// Возраст
    /// </summary>
    public int? Age => DateOfBirth.HasValue 
        ? DateTime.Today.Year - DateOfBirth.Value.Year - (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
        : null;

    /// <summary>
    /// Есть ли фото профиля
    /// </summary>
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);

    public PersonViewModel(IScreen hostScreen, Person person, IPersonService personService) : base(hostScreen)
    {
        Entity = person;
        _personService = personService;
    }

    public PersonViewModel(IScreen hostScreen, IPersonService personService) : base(hostScreen)
    {
        Entity = null;
        _personService = personService;
    }

    public void LoadFromEntity(Person person)
    {
        Entity = person;
        FirstName = person.FirstName;
        LastName = person.LastName;
        Email = person.Email ?? string.Empty;
        PhoneNumber = person.PhoneNumber;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address ?? string.Empty;
        ProfileImageUrl = person.ProfileImageUrl;
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }

    /// <summary>
    /// Преобразует ViewModel обратно в модель
    /// </summary>
    public Person ToModel()
    {
        return new Person
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DateOfBirth = DateOfBirth,
            Address = Address,
            ProfileImageUrl = ProfileImageUrl,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    private void UpdateFromPerson(Person person)
    {
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName;
        Email = person.Email;
        PhoneNumber = person.PhoneNumber;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address;
        ProfileImageUrl = person.ProfileImageUrl;
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }
} 

