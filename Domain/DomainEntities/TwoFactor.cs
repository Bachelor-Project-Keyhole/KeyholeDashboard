// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
namespace Domain.DomainEntities;

public class TwoFactor
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string? Identifier { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime ConfirmationCreationDate { get; set; }
}