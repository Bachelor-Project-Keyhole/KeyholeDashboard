

// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618
namespace Domain.Organization;

public class Organization
{
    public string Id { get; set; }
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public string ApiKey { get; set; }
    // ReSharper disable once CollectionNeverQueried.Global
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }

}






