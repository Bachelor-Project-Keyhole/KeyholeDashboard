namespace Application.DataPoint;

public interface IDataPointApplicationService
{
    Task<Contracts.v1.Dashboard.DashboardAndElementsResponse> TemplateDataPointsAndEntries(Domain.Dashboard.Dashboard dashboard, List<Domain.Template.Template> templates);
}