namespace Domain.RepositoryInterfaces;

public interface ITemplateRepository
{
    Task<Domain.Template.Template?> GetById(string id);
    Task<List<Domain.Template.Template>?> GetAllByDashboardId(string dashboardId);
    Task Insert(Domain.Template.Template template);
    Task Update(Domain.Template.Template template);
    Task DeleteTemplate(string id);
    Task RemoveAllTemplatesWithDashboardId(string dashboardId);
}