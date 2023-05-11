using Application.Email.Model;
using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.Organization;
using MongoDB.Bson;
using Repository.Datapoint;
using Repository.Organization;
using Repository.User.UserPersistence;

namespace WebApi.Registry;

static class MapperRegistry
{
    public static void RegisterAutoMapper(this IServiceCollection collection)
    {
        var config = new MapperConfiguration(cfg =>
        {
            // If we use mongo db this section will allow mapper to display objectId in string format.

            cfg.CreateMap<ObjectId, string>().ConvertUsing(o => o.ToString());
            cfg.CreateMap<string, ObjectId>().ConvertUsing(s => ObjectId.Parse(s));
            cfg.CreateMap<List<ObjectId>, List<string>>().ConvertUsing(o => o.Select(os => os.ToString()).ToList());
            cfg.CreateMap<List<string>, List<ObjectId>>().ConvertUsing(o => o.Select(ObjectId.Parse).ToList());

            #region User

            cfg.CreateMap<Domain.DomainEntities.User, UserPersistenceModel>()
                .ReverseMap();

            #endregion
            
            #region Email

            cfg.CreateMap<SendEmailRequest, WebApi.Controllers.V1.Email.Model.SendEmailRequest>()
                .ReverseMap();

            #endregion

            cfg.CreateMap<DataPointDto, DataPoint>().ReverseMap();
            cfg.CreateMap<DataPointEntity, DataPoint>().ReverseMap();
            cfg.CreateMap<DataPointEntry, DataPointEntryDto>().ReverseMap();
            cfg.CreateMap<DataPointEntry, DataPointEntryEntity>().ReverseMap();
            cfg.CreateMap<Organization, OrganizationEntity>().ReverseMap();
            cfg.CreateMap<TempOrganization, OrganizationEntity>().ReverseMap();
            cfg.CreateMap<TempOrganization, OrganizationDto>().ReverseMap();
        });
        collection.AddSingleton(config.CreateMapper()); 
    }
}