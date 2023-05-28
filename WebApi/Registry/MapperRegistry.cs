using AutoMapper;
using Contracts.v1.Dashboard;
using Contracts.v1.DataPoint;
using Contracts.v1.Organization;
using Contracts.v1.Template;
using Domain.Dashboard;
using Domain.Datapoint;
using Domain.Organization.OrganizationUserInvite;
using Domain.TwoFactor;
using Domain.Template;
using Domain.User;
using MongoDB.Bson;
using Repository.Dashboard;
using Repository.Datapoint;
using Repository.TwoFactor;
using Repository.Template;
using Repository.User.UserPersistence;
using WebApi.Controllers.Public.v1;

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

            cfg.CreateMap<User, UserPersistenceModel>().ReverseMap();
            cfg.CreateMap<RefreshToken, PersistenceRefreshToken>().ReverseMap();
            cfg.CreateMap<RefreshToken, Application.JWT.Model.JwtRefreshToken>().ReverseMap();
            cfg.CreateMap<User, OrganizationUsersResponse>().ReverseMap();
            cfg.CreateMap<TwoFactor, TwoFactorPersistence>().ReverseMap();
            #endregion

            #region Organization

            cfg.CreateMap<Domain.Organization.Organization, Repository.Organization.OrganizationEntity>().ReverseMap();
            
            cfg.CreateMap<Domain.Organization.Organization, Repository.Organization.OrganizationPersistenceModel>().ReverseMap();
            cfg.CreateMap<OrganizationUserInvites, Repository.OrganizationUserInvite.OrganizationUserInvitePersistence>().ReverseMap();
            cfg.CreateMap<OrganizationUserInvites, PendingUserInvitationResponse>().ReverseMap();


            #endregion

            #region Dashboard

            cfg.CreateMap<Dashboard, DashboardPersistenceModel>().ReverseMap();
            cfg.CreateMap<Dashboard, DashboardResponse>();

            #endregion

            #region Template

            cfg.CreateMap<CreateTemplateRequest, Template>().ReverseMap();
            cfg.CreateMap<UpdateTemplateRequest, Template>().ReverseMap();
            cfg.CreateMap<Template, TemplatePersistenceModel>().ReverseMap();
            cfg.CreateMap<Template, TemplateResponse>().ReverseMap();


            #endregion

            cfg.CreateMap<DataPointDto, DataPoint>().ReverseMap();
            cfg.CreateMap<DataPointDisplayNameResponse, DataPoint>().ReverseMap();
            cfg.CreateMap<CreateDataPointRequest, DataPoint>().ReverseMap();
            cfg.CreateMap<DataPointEntity, DataPoint>().ReverseMap();
            cfg.CreateMap<DataPointEntry, PushDataPointEntryRequest>().ReverseMap();
            cfg.CreateMap<DataPointEntry, HistoricDataPointEntryRequest>().ReverseMap();
            cfg.CreateMap<DataPointEntry, DataPointEntryEntity>().ReverseMap();
            cfg.CreateMap<DataPointEntry, DataPointEntryResponse>().ReverseMap();
            cfg.CreateMap<Formula, FormulaDto>().ReverseMap();
            cfg.CreateMap<MathOperation, string>().ConvertUsing(e => e.ToString());
            cfg.CreateMap<string, MathOperation>().ConvertUsing(s => Enum.Parse<MathOperation>(s));
        });
        collection.AddSingleton(config.CreateMapper()); 
    }
}