using MongoDB.Bson;

namespace Repository.TwoFactor.TwoFactorWriteModel;

public interface ITwoFactorWriteModel
{
    Task Insert(TwoFactorPersistence twoFactor);
    Task Delete(ObjectId id);
}