using MongoDB.Bson;

namespace Domain;

public static class IdGenerator
{
    private static readonly object IdLock = new();
    
    public static string GenerateId()
    {
        lock (IdLock)
        {
            return ObjectId.GenerateNewId().ToString();    
        }
    }
}