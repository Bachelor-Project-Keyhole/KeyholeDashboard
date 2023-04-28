using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Repository;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    ObjectId Id { get; set; }
}