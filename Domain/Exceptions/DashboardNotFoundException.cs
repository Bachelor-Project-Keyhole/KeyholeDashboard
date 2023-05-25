namespace Domain.Exceptions;

public class DashboardNotFoundException : Exception
{
    public DashboardNotFoundException(string message): base(message) 
    {}
}