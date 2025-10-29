using DbFirst.Models;
using System.ComponentModel.DataAnnotations;

namespace FitnessCentrApp.ViewModels.Base.Models;

public class TrainerViewModel : ValidatableViewModel
{
    public Trainer Model { get; }

    public TrainerViewModel(Trainer model)
    {
        Model = model;
    }

    // Пустой публичный конструктор (требование new() в BaseCrudViewModel)
    public TrainerViewModel()
    {
        Model = new Trainer()
        {
            FullName = string.Empty,
            Phone = string.Empty,
            Email = string.Empty,
            Education = string.Empty,
            WorkExperience = string.Empty,
            SportsAchievements = string.Empty,
            Specialization = string.Empty,
            Salary = 0,
            PhotoPath = string.Empty,
            BranchID = 1
        };
    }

    [Required(ErrorMessage = "ФИО обязательно")]
    [StringLength(100)]
    public string FullName
    {
        get => Model.FullName;
        set
        {
            Model.FullName = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(FullName));
        }
    }

    [Required(ErrorMessage = "Телефон обязателен")]
    [StringLength(20)]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string Phone
    {
        get => Model.Phone;
        set
        {
            Model.Phone = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Phone));
        }
    }

    [Required(ErrorMessage = "Email обязателен")]
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    public string Email
    {
        get => Model.Email;
        set
        {
            Model.Email = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Email));
        }
    }

    [Required(ErrorMessage = "Специализация обязательна")]
    [StringLength(100)]
    public string Specialization
    {
        get => Model.Specialization;
        set
        {
            Model.Specialization = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Specialization));
        }
    }

    [Required]
    [StringLength(500)]
    public string Education
    {
        get => Model.Education;
        set
        {
            Model.Education = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Education));
        }
    }

    [Required]
    [StringLength(1000)]
    public string WorkExperience
    {
        get => Model.WorkExperience;
        set
        {
            Model.WorkExperience = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(WorkExperience));
        }
    }

    [Required]
    [StringLength(1000)]
    public string SportsAchievements
    {
        get => Model.SportsAchievements;
        set
        {
            Model.SportsAchievements = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(SportsAchievements));
        }
    }

    [Range(0, 1000000, ErrorMessage = "Зарплата должна быть положительной")]
    public decimal Salary
    {
        get => Model.Salary;
        set
        {
            Model.Salary = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Salary));
        }
    }

    public int BranchID
    {
        get => Model.BranchID;
        set
        {
            Model.BranchID = value;
            OnPropertyChanged();
        }
    }

    public string PhotoPath
    {
        get => Model.PhotoPath ?? "";
        set
        {
            Model.PhotoPath = value;
            OnPropertyChanged();
        }
    }
}
