using MongoDB.Bson;

namespace Repository;

public abstract class Document : IDocument
{
    public ObjectId Id { get; set; }
}