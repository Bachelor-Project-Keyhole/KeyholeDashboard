namespace Domain.Datapoint;

public interface IDataPointEntryRepository
{
    Task AddDataPointEntry(DataPointEntry dataPointEntry);
}