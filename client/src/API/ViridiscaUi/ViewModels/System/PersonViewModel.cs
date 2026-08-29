using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления данными Person
/// </summary>
public class PersonViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string? MiddleName { get; set; }
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string? Phone { get; set; }
    [Reactive] public DateTime? DateOfBirth { get; set; }
    [Reactive] public string? Address { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    // Computed properties
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    public string DisplayName => $"{FirstName} {LastName}";

    public PersonViewModel() { }

    public PersonViewModel(Person person)
    {
        Uid = person.Uid;
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName;
        Email = person.Email;
        Phone = person.Phone;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address;
        IsActive = person.IsActive;
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }

    public Person ToPerson()
    {
        return new Person
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            Phone = Phone,
            DateOfBirth = DateOfBirth,
            Address = Address,
            IsActive = IsActive,
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
        Phone = person.Phone;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address;
        IsActive = person.IsActive;
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }
} 

