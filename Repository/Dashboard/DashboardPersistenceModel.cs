namespace Repository.Dashboard;

[BsonCollection("dashboard")]
public class DashboardPersistenceModel : Document
{
    public string OrganizationId { get; set; }
    public string Name { get; set; }
}