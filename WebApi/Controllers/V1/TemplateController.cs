using AutoMapper;
using Contracts;
using Domain.Template;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

[Route("api/v1/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ITemplateDomainService _templateDomainService;

    public TemplateController(IMapper mapper, ITemplateDomainService templateDomainService)
    {
        _mapper = mapper;
        _templateDomainService = templateDomainService;
    }

    [HttpGet]
    public async Task<PreviewDataDto> GetPreviewDataDto([FromBody] PreviewDataRequestDto previewDataRequestDto)
    {
        await _templateDomainService.GetPreviewData(previewDataRequestDto.OrganizationId,
            previewDataRequestDto.DataPointId, previewDataRequestDto.DisplayType, previewDataRequestDto.TimeSpanInDays);
        return new PreviewDataDto();
    }
}