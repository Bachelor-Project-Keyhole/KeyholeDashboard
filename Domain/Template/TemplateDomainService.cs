namespace Domain.Template;

public class TemplateDomainService : ITemplateDomainService
{
    public Task<PreviewData> GetPreviewData(string organizationId, string dataPointId, string displayType, int timeSpanInDays)
    {
        throw new NotImplementedException();
    }
}