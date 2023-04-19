using MongoDB.Bson;

namespace Repository.TwoFactor;

public class TwoFactorPersistence
{
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public string? Identifier { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime ConfirmationCreationDate { get; set; }
}