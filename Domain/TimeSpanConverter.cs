using System.ComponentModel;
using Domain.Template;

namespace Domain;

public static class TimeSpanConverter
{
    public static DateTime CalculatePeriodBoundary(int timespan, TimeUnit timeUnit)
    {
        var result = DateTime.Now;
        switch (timeUnit)
        {
            case TimeUnit.Day:
                return result.AddDays(-timespan);
            case TimeUnit.Week:
                return result.AddDays(-timespan*7);
            case TimeUnit.Month:
                return result.AddMonths(-timespan);
            case TimeUnit.Year:
                return result.AddYears(-timespan);
            default:
                throw new InvalidEnumArgumentException();
        }
    } 
}