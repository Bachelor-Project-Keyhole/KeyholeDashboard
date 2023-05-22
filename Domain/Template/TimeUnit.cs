using System.ComponentModel.DataAnnotations;

namespace Domain.Template;

public enum TimeUnit
{
    [Display(Name = "Day")]
    Day,
    [Display(Name = "Week")]
    Week,
    [Display(Name = "Month")]
    Month,
    [Display(Name = "Year")]
    Year
}