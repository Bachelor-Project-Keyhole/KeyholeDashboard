namespace Domain.Exceptions;

public class DeleteDataPointWarningException : Exception
{
    public DeleteDataPointWarningException(string dataPointKey) : base(
        $"Deletion of data point with key \'{dataPointKey}\' will result with deletion of all entries with matching key")
    {
    }
}