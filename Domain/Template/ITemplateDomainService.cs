namespace Domain.Template;

public interface ITemplateDomainService
{
    Task<PreviewData> GetPreviewData(string organizationId, string dataPointId, string displayType, int timeSpanInDays);
}