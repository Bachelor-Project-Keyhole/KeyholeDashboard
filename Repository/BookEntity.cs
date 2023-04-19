using MongoDB.Bson.Serialization.Attributes;

namespace Repository;

public class BookEntity
{
    [BsonId]
    public string ISBN { get; set; }
    public string Title { get; set; }

    public string Author { get; set; }
}