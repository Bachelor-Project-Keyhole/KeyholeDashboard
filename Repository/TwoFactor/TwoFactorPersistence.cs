using MongoDB.Bson;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS0108, CS0114

namespace Repository.TwoFactor;

[BsonCollection("twoFactor")]
public class TwoFactorPersistence : Document
{
    public string UserId { get; set; }
    public string? Identifier { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime ConfirmationCreationDate { get; set; }
}