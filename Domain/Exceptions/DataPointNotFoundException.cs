namespace Domain.Exceptions;

public class DataPointNotFoundException : Exception
{
    public DataPointNotFoundException(string dataPointId) : base(
        $"Data point with id: \'{dataPointId}\' was not found")
    {}
}