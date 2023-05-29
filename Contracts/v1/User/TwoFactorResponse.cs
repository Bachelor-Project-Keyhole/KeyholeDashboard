#pragma warning disable CS8618
namespace Contracts.v1.User;

public class TwoFactorResponse
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string? Identifier { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime ConfirmationCreationDate { get; set; }
}