using AutoMapper;
using Contracts.@public;
using Domain.Datapoint;
using Domain.Organization;

namespace Application.DataPoint;

public class DataPointApplicationService : IDataPointApplicationService
{
    private readonly IOrganizationDomainService _organizationDomainService;
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IMapper _mapper;

    public DataPointApplicationService(IOrganizationDomainService organizationDomainService,
        IDataPointDomainService dataPointDomainService, IMapper mapper)
    {
        _organizationDomainService = organizationDomainService;
        _dataPointDomainService = dataPointDomainService;
        _mapper = mapper;
    }

    public async Task AddDataPointEntry(string dataPointKey, double value, string apiKey)
    {
        var organization = await _organizationDomainService.GetOrganizationByApiKey(apiKey);
        await _dataPointDomainService.AddDataPointEntry(dataPointKey, value, organization.Id);
    }

    public async Task AddDataPointEntries(PushDataPointEntryRequest[] dataPointEntryDtos, string apiKey)
    {
        var organization = await _organizationDomainService.GetOrganizationByApiKey(apiKey);
        var dataPointEntries = _mapper.Map<DataPointEntry[]>(dataPointEntryDtos);
        
        await _dataPointDomainService.AddDataPointEntries(dataPointEntries, organization.Id);
    }

    public async Task AddHistoricDataPointEntries(HistoricDataPointEntryRequest[] dataPointEntryDtos, string apiKey)
    {
        var organization = await _organizationDomainService.GetOrganizationByApiKey(apiKey);
        var dataPointEntries = _mapper.Map<DataPointEntry[]>(dataPointEntryDtos);
        await _dataPointDomainService.AddHistoricDataPointEntries(dataPointEntries, organization.Id);
    }
}