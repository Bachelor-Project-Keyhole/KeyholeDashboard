namespace Domain.Exceptions;

public class DataPointKeyNotFoundException : Exception
{
    public DataPointKeyNotFoundException(string dataPointKey) : base(
        $"Data point key with value: \'{dataPointKey}\' was not found")
    {}
}