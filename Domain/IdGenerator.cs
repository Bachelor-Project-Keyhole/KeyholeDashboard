using MongoDB.Bson;

namespace Domain;

public static class IdGenerator
{
    public static string GenerateId()
    {
        return ObjectId.GenerateNewId().ToString();
    }
}